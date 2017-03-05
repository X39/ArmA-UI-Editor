using System;
using ICSharpCode.AvalonEdit.Document;

namespace ArmA.Studio.DataContext.TextEditorUtil
{
    public partial class LinterInfo : SyntaxError
    {
        public int Line { get; set; }
        public int LineOffset { get; set; }
        public ESeverity Severity { get; set; }
        public string FileName { get; set; }
    }
}