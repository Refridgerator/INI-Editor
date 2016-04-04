using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace IniEditor
{
    #region Format definition

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INISECTION")]
    [Name("INISECTION")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class IniSection : ClassificationFormatDefinition
    {
        public IniSection()
        {
            this.DisplayName = "Ini Section"; //human readable version of the name
            this.ForegroundColor = Colors.BlueViolet;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INICOMMENT")]
    [Name("INICOMMENT")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class IniComment : ClassificationFormatDefinition
    {
        public IniComment()
        {
            this.DisplayName = "Ini Comment"; //human readable version of the name
            this.ForegroundColor = Colors.Gray;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INIKEY")]
    [Name("INIKEY")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class IniKey : ClassificationFormatDefinition
    {
        public IniKey()
        {
            this.DisplayName = "Ini Key"; //human readable version of the name
            this.ForegroundColor = Colors.DarkBlue;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "INISTRINGVALUE")]
    [Name("INISTRINGVALUE")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class IniStringValue : ClassificationFormatDefinition
    {
        public IniStringValue()
        {
            this.DisplayName = "Ini String Value"; //human readable version of the name
            this.ForegroundColor = Colors.DarkGreen;
            this.ForegroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ININUMBERVALUE")]
    [Name("ININUMBERVALUE")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class IniNumberValue : ClassificationFormatDefinition
    {
        public IniNumberValue()
        {
            this.DisplayName = "Ini Number Value"; //human readable version of the name
            this.ForegroundColor = Colors.DarkRed;
            this.ForegroundCustomizable = true;
        }
    }
  
    #endregion //Format definition
}
