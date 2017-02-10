using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit.Highlighting;

namespace ArmA.Studio.DataContext
{
    public class ConfigEditorDocument : TextEditorDocument
    {
        private static IHighlightingDefinition ThisSyntaxName { get; set; }
        static ConfigEditorDocument()
        {
            ThisSyntaxName = LoadAvalonEditSyntaxFiles(System.IO.Path.Combine(App.SyntaxFilesPath, "armaconfig.xshd"));
        }
        public override string[] SupportedFileExtensions { get { return new string[] { ".hpp", ".cpp", ".ext" }; } }
        public override IHighlightingDefinition SyntaxDefinition { get { return ThisSyntaxName; } }


    }
}