namespace IniEditor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ITaggerProvider))]
    [ContentType("ini")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class IniClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("ini")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition IniContentType = null;

        [Export]
        [FileExtension(".ini")]
        [ContentType("ini")]
        internal static FileExtensionToContentTypeDefinition IniFileType = null;
   
        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<IniTokenTag> IniTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<IniTokenTag>(buffer);

            return new IniClassifier(buffer, IniTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class IniClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<IniTokenTag> _aggregator;
        IDictionary<IniTokens, IClassificationType> _iniTypes;

        internal IniClassifier(ITextBuffer buffer,
                               ITagAggregator<IniTokenTag> IniTagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = IniTagAggregator;
            _iniTypes = new Dictionary<IniTokens, IClassificationType>();

            _iniTypes[IniTokens.TEXT] = typeService.GetClassificationType("INITEXT");
            _iniTypes[IniTokens.COMMENT] = typeService.GetClassificationType("INICOMMENT");
            _iniTypes[IniTokens.SECTION] = typeService.GetClassificationType("INISECTION");
            _iniTypes[IniTokens.KEY] = typeService.GetClassificationType("INIKEY");
            _iniTypes[IniTokens.STRINGVALUE] = typeService.GetClassificationType("INISTRINGVALUE");
            _iniTypes[IniTokens.NUMBERVALUE] = typeService.GetClassificationType("ININUMBERVALUE");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            ITextSnapshot snapshot = spans[0].Snapshot;
            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return new TagSpan<ClassificationTag>(tagSpans[0],
                    new ClassificationTag(_iniTypes[tagSpan.Tag.type]));
            }
        }
    }


 


}
