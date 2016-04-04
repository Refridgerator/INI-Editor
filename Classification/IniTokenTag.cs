// Copyright (c) Microsoft Corporation
// All rights reserved

namespace IniEditor
{
    using System.Reflection;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    [Export(typeof(ITaggerProvider))]
    [ContentType("ini")]
    [TagType(typeof(IniTokenTag))]
    internal sealed class IniTokenTagProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new IniTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class IniTokenTag : ITag 
    {
        public IniTokens type { get; private set; }

        public IniTokenTag(IniTokens type)
        {
            this.type = type;
        }
    }

    internal sealed class IniTokenTagger : ITagger<IniTokenTag>
    {
        ITextBuffer _buffer;
        Regex rx_section = new Regex(@"^\[.+[^;]\]", RegexOptions.Compiled);
        Regex rx_eq = new Regex(@"\s*=\s*", RegexOptions.Compiled);
        Regex rx_comment = new Regex(@"\s*;", RegexOptions.Compiled); // @"(^;|;$|;\s+)"
        Regex rx_number = new Regex(@"^(-?\d+(\.\d+)?|0[xX][0-9a-fA-F]+)$", RegexOptions.Compiled);
        
        internal IniTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<IniTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            List<TagSpan<IniTokenTag>> tags = new List<TagSpan<IniTokenTag>>();
            foreach (SnapshotSpan curSpan in spans)
            {
                SnapshotSpan tokenSpan;
                ITextSnapshotLine textSnapshotLine = curSpan.Start.GetContainingLine();
                string text_line = textSnapshotLine.GetText().Trim();
                int cur_pos = textSnapshotLine.Start.Position;
                //
                Match m_section = rx_section.Match(text_line);
                Match m_eq = rx_eq.Match(text_line);
                Match m_comment = rx_comment.Match(text_line);

                int comment_pos = m_comment.Index;
                int eq_pos = m_eq.Index;
                int eq_width = m_eq.Length;
                // comment
                if (m_comment.Success)
                {
                    tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos + comment_pos, textSnapshotLine.Length - comment_pos));
                    if (tokenSpan.IntersectsWith(curSpan))
                        tags.Add(new TagSpan<IniTokenTag>(tokenSpan, new IniTokenTag(IniTokens.COMMENT)));

                    if (m_eq.Index > comment_pos && !m_section.Success) continue;
                }

                // section               
                if (m_section.Success)
                {
                    tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos, m_section.Length));
                    if (tokenSpan.IntersectsWith(curSpan))
                        tags.Add(new TagSpan<IniTokenTag>(tokenSpan, new IniTokenTag(IniTokens.SECTION)));
                    continue;
                }
               
                // key - value
                if (m_eq.Success)
                {
                    // key
                    tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos, eq_pos));
                    if (tokenSpan.IntersectsWith(curSpan))
                        tags.Add(new TagSpan<IniTokenTag>(tokenSpan, new IniTokenTag(IniTokens.KEY)));
                    // value
                    if (m_comment.Success && (comment_pos > eq_pos))
                        tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos + eq_pos + eq_width, comment_pos - eq_pos - eq_width));
                    else
                        tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos + eq_pos + eq_width, textSnapshotLine.Length - eq_pos - eq_width));

                    if (tokenSpan.IntersectsWith(curSpan))
                    {
                        string v = tokenSpan.GetText();
                        if(rx_number.Match(v).Success)
                        tags.Add(new TagSpan<IniTokenTag>(tokenSpan, new IniTokenTag(IniTokens.NUMBERVALUE)));
                        else
                            tags.Add(new TagSpan<IniTokenTag>(tokenSpan, new IniTokenTag(IniTokens.STRINGVALUE)));
                    }
                    continue;
                }

                // unknown
                tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(cur_pos, textSnapshotLine.Length));
                if (tokenSpan.IntersectsWith(curSpan))
                    tags.Add(new TagSpan<IniTokenTag>(tokenSpan, new IniTokenTag(IniTokens.TEXT)));
            }
            //
            foreach (var tag in tags)
                yield return tag;
        }
    }
}
