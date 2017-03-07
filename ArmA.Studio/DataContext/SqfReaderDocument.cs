using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ArmA.Studio.DataContext.TextEditorUtil;
using ArmA.Studio.UI;
using ICSharpCode.AvalonEdit.Highlighting;
using RealVirtuality.SQF.Parser;

namespace ArmA.Studio.DataContext
{
    public class SQFReaderDocument : TextEditorDocument
    {
        private static IHighlightingDefinition ThisSyntaxName { get; set; }
        static SQFReaderDocument()
        {
            ThisSyntaxName = LoadAvalonEditSyntaxFiles(Path.Combine(App.SyntaxFilesPath, "sqf.xshd"));
        }
        public override string[] SupportedFileExtensions { get { return new string[] { ".sqf" }; } }
        public override IHighlightingDefinition SyntaxDefinition { get { return ThisSyntaxName; } }

        public BreakPointMargin BreakPointMargin { get; private set; }

        protected override void OnTextEditorSet()
        {
            var sf = Workspace.CurrentWorkspace.CurrentSolution.GetOrCreateFileReference(this.FilePath) as SolutionUtil.SolutionFile;
            this.BreakPointMargin = new BreakPointMargin(sf);
            this.Editor.TextArea.TextView.BackgroundRenderers.Add(this.BreakPointMargin);
            this.Editor.TextArea.LeftMargins.Insert(0, BreakPointMargin);
        }
        protected override IEnumerable<LinterInfo> GetLinterInformations(MemoryStream memstream)
        {
            var inputStream = new Antlr4.Runtime.AntlrInputStream(memstream);

            var lexer = new RealVirtuality.SQF.ANTLR.Parser.sqfLexer(inputStream, new RealVirtuality.SQF.SqfDefinition[] { });
            var commonTokenStream = new Antlr4.Runtime.CommonTokenStream(lexer);
            var p = new RealVirtuality.SQF.ANTLR.Parser.sqfParser(commonTokenStream);
            var listener = new RealVirtuality.SQF.ANTLR.SqfListener();
            p.AddParseListener(listener);
            memstream.Seek(0, SeekOrigin.Begin);

            p.RemoveErrorListeners();
            var se = new List<LinterInfo>();
            p.AddErrorListener(new RealVirtuality.SQF.ANTLR.ErrorListener((recognizer, token, line, charPositionInLine, msg, ex) =>
            {
                se.Add(new LinterInfo()
                {
                    StartOffset = token.StartIndex,
                    Length = token.Text.Length,
                    Message = msg,
                    Severity = ESeverity.Error,
                    Line = line,
                    LineOffset = charPositionInLine,
                    FileName = Path.GetFileName(this.FilePath)
                });
            }));
            
            p.sqf();
            return se;
        }
    }
}