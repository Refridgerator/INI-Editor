﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;

namespace IniEditor
{
    internal sealed class IniOutliningTagger : ITagger<IOutliningRegionTag>
    {
        class Region
        {
            public int StartLine { get; set; }
            public int EndLine { get; set; }
            public int StartOffset { get; set; }
            public string ellipsis = "...";    //the characters that are displayed when the region is collapsed 
            public string hover_text = ""; //the contents of the tooltip for the collapsed span  
            public bool collapsed = false;
        }

        ITextBuffer buffer;
        ITextSnapshot snapshot;
        List<Region> regions;
        Regex rx_section = new Regex(@"^\[.+[^;]\]", RegexOptions.Compiled);

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IniOutliningTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            this.snapshot = buffer.CurrentSnapshot;
            this.regions = new List<Region>();
            this.ReParse();
            this.buffer.Changed += BufferChanged;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            List<Region> currentRegions = this.regions;
            ITextSnapshot currentSnapshot = this.snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;

            foreach (var region in currentRegions)
            {
                if (region.StartLine <= endLineNumber && region.EndLine >= startLineNumber)
                {
                    var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                    var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End),
                        new OutliningRegionTag(region.collapsed, false, region.ellipsis, region.ellipsis));
                }
            }
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now 
            // (we'll eventually get another change event). 
            if (e.After != buffer.CurrentSnapshot) return;
            this.ReParse();
        }

        void ReParse()
        {
            ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
            List<Region> newRegions = new List<Region>();

            //keep the current (deepest) partial region, which will have 
            // references to any parent partial regions.
            Region currentRegion = null;

            //int last_not_comment_line = -1;
            int last_comment_line = -1;
            int last_comment_above_empty_line = -1;
            int last_empty_line = -1;
            int last_keyvalue_line = -1;

            foreach (var line in newSnapshot.Lines)
            {
                string linetext = line.GetText().Trim();

                Match m = rx_section.Match(linetext);

                if (linetext.StartsWith(";"))
                    last_comment_line = line.LineNumber;
                else
                    if (string.IsNullOrEmpty(linetext))
                    {
                        last_empty_line = line.LineNumber;
                        last_comment_above_empty_line = last_comment_line;
                    }
                    else
                        if (!m.Success)
                            last_keyvalue_line = line.LineNumber;

                if (m.Success)
                {
                    if (currentRegion != null)
                    {
                        currentRegion.EndLine = Math.Max(last_keyvalue_line, last_comment_above_empty_line);
                        if (currentRegion.EndLine <= currentRegion.StartLine)
                            newRegions.Remove(currentRegion);
                    }

                    last_comment_line = -1;
                    last_comment_above_empty_line = -1;
                    last_empty_line = -1;
                    last_keyvalue_line = -1;

                    currentRegion = new Region()
                     {
                         StartLine = line.LineNumber,
                         EndLine = -1,
                         StartOffset = 0,
                         ellipsis = m.Value,
                         collapsed = false
                     };

                    newRegions.Add(currentRegion);
                }
            }
            if (currentRegion != null)
                if (currentRegion.EndLine == -1)
                {
                    //if (last_comment_line > last_empty_line)
                    //    currentRegion.EndLine = last_keyvalue_line;
                    //else
                    currentRegion.EndLine = Math.Max(last_keyvalue_line, last_comment_above_empty_line);
                    if (currentRegion.EndLine <= currentRegion.StartLine)
                        newRegions.Remove(currentRegion);
                }

            //determine the changed span, and send a changed event with the new spans
            List<Span> oldSpans =
                new List<Span>(this.regions.Select(r => AsSnapshotSpan(r, this.snapshot)
                    .TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive)
                    .Span));
            List<Span> newSpans =
                    new List<Span>(newRegions.Select(r => AsSnapshotSpan(r, newSnapshot).Span));

            NormalizedSpanCollection oldSpanCollection = new NormalizedSpanCollection(oldSpans);
            NormalizedSpanCollection newSpanCollection = new NormalizedSpanCollection(newSpans);

            //the changed regions are regions that appear in one set or the other, but not both.
            NormalizedSpanCollection removed =
            NormalizedSpanCollection.Difference(oldSpanCollection, newSpanCollection);

            int changeStart = int.MaxValue;
            int changeEnd = -1;

            if (removed.Count > 0)
            {
                changeStart = removed[0].Start;
                changeEnd = removed[removed.Count - 1].End;
            }

            if (newSpans.Count > 0)
            {
                changeStart = Math.Min(changeStart, newSpans[0].Start);
                changeEnd = Math.Max(changeEnd, newSpans[newSpans.Count - 1].End);
            }

            this.snapshot = newSnapshot;
            this.regions = newRegions;

            if (changeStart <= changeEnd)
            {
                ITextSnapshot snap = this.snapshot;
                if (this.TagsChanged != null)
                    this.TagsChanged(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(this.snapshot, Span.FromBounds(changeStart, changeEnd))));
            }
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
            var endLine = (region.StartLine == region.EndLine) ?
                startLine : snapshot.GetLineFromLineNumber(region.EndLine);

            return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
        }
    }
}
