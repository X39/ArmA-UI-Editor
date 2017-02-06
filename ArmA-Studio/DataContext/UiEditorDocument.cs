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
    public class UiEditorDocument : TextEditorDocument
    {
        private static IHighlightingDefinition ThisSyntaxName { get; set; }
        static UiEditorDocument()
        {
            ThisSyntaxName = LoadAvalonEditSyntaxFiles(System.IO.Path.Combine(App.SyntaxFilesPath, "armaconfig.xshd"));
        }
        public override string[] SupportedFileExtensions { get { return new string[] { ".cpp", ".ext", ".hpp" }; } }
        public override IHighlightingDefinition SyntaxDefinition { get { return ThisSyntaxName; } }

        public int CurrentTabIndex
        {
            set
            {
                if (value == 0)
                {
                    //ToDo: Clear UI-Controls and ConfigTree
                }
                else
                {
                    //ToDo: Build UI-Controls and ConfigTree
                }
            }
        }

        protected override void OnTextChanged(object param)
        {
            base.OnTextChanged(param);
        }

        public UiEditorDocument()
        {
            
        }
    }
}