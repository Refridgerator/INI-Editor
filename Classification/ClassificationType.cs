using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace IniEditor
{
    internal static class OrdinaryClassificationDefinition
    {
        #region Type definition

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INITEXT")]
        internal static ClassificationTypeDefinition TEXT = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INISECTION")]
        internal static ClassificationTypeDefinition SECTION = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INIKEY")]
        internal static ClassificationTypeDefinition KEY = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INICOMMENT")]
        internal static ClassificationTypeDefinition COMMENT = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("INISTRINGVALUE")]
        internal static ClassificationTypeDefinition STRINGVALUE = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("ININUMBERVALUE")]
        internal static ClassificationTypeDefinition NUMBERVALUE = null;

        #endregion
    }
}
