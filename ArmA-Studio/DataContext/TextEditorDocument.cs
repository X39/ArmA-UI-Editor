using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Windows.Controls;
using ArmA.Studio.DataContext.TextEditorUtil;
using System.Windows.Controls.Primitives;

namespace ArmA.Studio.DataContext
{
    public class TextEditorDocument : DocumentBase
    {
        private static DataTemplate ThisTemplate { get; set; }
        static TextEditorDocument()
        {
            ThisTemplate = GetDataTemplateFromAssemblyRes("ArmA.Studio.UI.DataTemplates.TextEditorDocumentTemplate.xaml");
        }
        public override string[] SupportedFileExtensions { get { return new string[] { ".txt" }; } }
        public override DataTemplate Template { get { return ThisTemplate; } }
        public virtual IHighlightingDefinition SyntaxDefinition { get { return null; } }


        public override string Title { get { return this.HasChanges ? string.Concat(Path.GetFileName(this.FilePath), '*') : Path.GetFileName(this.FilePath); } }
        public override string FilePath { get { return this._FilePath; } }
        private string _FilePath;
        public void SetFilePath(string title)
        {
            this._FilePath = title;
            this.RaisePropertyChanged("Title");
            this.RaisePropertyChanged("FilePath");
        }

        public TextDocument Document { get { return this._Document; } set { this._Document = value; this.RaisePropertyChanged(); } }
        private TextDocument _Document;

        public ICSharpCode.AvalonEdit.TextEditor Editor { get { return this._Editor; } set { this._Editor = value; this.RaisePropertyChanged(); this.OnTextEditorSet(); } }
        private ICSharpCode.AvalonEdit.TextEditor _Editor;

        public bool CmdKeyDownHandledValue { get { return this._CmdKeyDownHandledValue; } set { this._CmdKeyDownHandledValue = value; this.RaisePropertyChanged(); } }
        private bool _CmdKeyDownHandledValue;
        public string IntelliSenseCurrentWord { get { return this._IntelliSenseCurrentWord; } set { this._IntelliSenseCurrentWord = value; this.RaisePropertyChanged(); } }
        private string _IntelliSenseCurrentWord;

        public ICommand CmdTextChanged { get; private set; }
        public ICommand CmdKeyDown { get; private set; }
        public ICommand CmdTextEditorInitialized { get; private set; }
        public ICommand CmdIntelliSensePopupInitialized { get; private set; }
        public ICommand CmdEditorPreviewMouseDown { get; private set; }

        public SolutionUtil.SolutionFileBase SFBRef { get; private set; }

        internal UI.SyntaxErrorBackgroundRenderer SyntaxErrorRenderer { get; private set; }
        public IEnumerable<SyntaxError> SyntaxErrors { get; private set; }
        public IList<IntelliSenseEntry> IntelliSenseEntries { get { return this._IntelliSenseEntries; } set { this._IntelliSenseEntries = value; this.RaisePropertyChanged(); } }
        public IList<IntelliSenseEntry> _IntelliSenseEntries;

        private ToolTip EditorTooltip;
        private Popup IntelliSensePopup;

        public TextEditorDocument()
        {
            this.EditorTooltip = new ToolTip();
            this.SyntaxErrorRenderer = new UI.SyntaxErrorBackgroundRenderer();
            this.CmdTextChanged = new UI.Commands.RelayCommand(OnTextChanged);
            this.CmdKeyDown = new UI.Commands.RelayCommand(OnKeyDown);
            this.CmdTextEditorInitialized = new UI.Commands.RelayCommand((p) =>
            {
                this.Editor = p as ICSharpCode.AvalonEdit.TextEditor;
                this.Editor.MouseHover += Editor_MouseHover;
                this.Editor.MouseHoverStopped += Editor_MouseHoverStopped;
                ;
                this.Editor.TextArea.TextView.BackgroundRenderers.Add(new UI.LineHighlighterBackgroundRenderer(this.Editor));
                this.Editor.TextArea.TextView.BackgroundRenderers.Add(this.SyntaxErrorRenderer);
            });
            this.CmdIntelliSensePopupInitialized = new UI.Commands.RelayCommand((p) => this.IntelliSensePopup = p as Popup);
            this.CmdEditorPreviewMouseDown = new UI.Commands.RelayCommand((p) => this.IntelliSensePopup.IsOpen = false);
            this._Document = new TextDocument();
            this._Document.TextChanged += Document_TextChanged;
        }


        private void Editor_MouseHover(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this.Editor);
            var textViewPos = this.Editor.GetPositionFromPoint(pos);
            if (textViewPos.HasValue)
            {
                var textOffset = this.Document.GetOffset(textViewPos.Value.Location);
                var errors = this.SyntaxErrors;
                foreach(var error in this.SyntaxErrors)
                {
                    if(error.StartOffset <= textOffset && error.EndOffset >= textOffset)
                    {
                        this.EditorTooltip.PlacementTarget = this.Editor;
                        this.EditorTooltip.Content = error.Message;
                        this.EditorTooltip.IsOpen = true;
                        break;
                    }
                }
            }
        }
        private void Editor_MouseHoverStopped(object sender, MouseEventArgs e)
        {
            this.EditorTooltip.IsOpen = false;
        }

        private void Document_TextChanged(object sender, EventArgs e)
        {
            this.SyntaxErrors = SyntaxErrorRenderer.SyntaxErrors = this.GetSyntaxErrors();
            this.ShowIntelliSense();
        }

        protected virtual void OnTextChanged(object param)
        {
            this.HasChanges = true;
            this.RaisePropertyChanged("Title");
        }
        protected virtual void OnTextEditorSet() { }
        protected virtual void OnKeyDown(object param)
        {
            this.CmdKeyDownHandledValue = true;
            if (Keyboard.IsKeyDown(Key.S) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                this.SaveDocument(this.FilePath);
            }
            else if (Keyboard.IsKeyDown(Key.Space) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                this.ShowIntelliSense();
            }
            else
            {
                this.IntelliSensePopup.IsOpen = false;
                this.CmdKeyDownHandledValue = false;
            }
        }

        private void ShowIntelliSense()
        {
            if (this.Editor == null)
                return;
            string curWord = string.Empty;
            for (var i = this.Editor.CaretOffset - 1; i >= 0; i--)
            {
                var c = this.Document.GetCharAt(i);
                if (char.IsWhiteSpace(c))
                {
                    curWord = this.Document.GetText(i + 1, this.Editor.CaretOffset - i - 1);
                    break;
                }
            }
            this.IntelliSenseEntries = this.GetIntelliSenseEntries(this.Document, curWord, this.Editor.CaretOffset);
            if (this.IntelliSenseEntries.Count > 0)
            {
                this.IntelliSenseCurrentWord = curWord;
                this.IntelliSensePopup.DataContext = this;
                var pos = this.Editor.TextArea.TextView.GetVisualPosition(this.Editor.TextArea.Caret.Position, ICSharpCode.AvalonEdit.Rendering.VisualYPosition.TextBottom);
                this.IntelliSensePopup.HorizontalOffset = pos.X + this.Editor.TextArea.LeftMargins.Sum((it) => it.RenderSize.Width) + 6;
                this.IntelliSensePopup.VerticalOffset = pos.Y - this.Editor.ActualHeight;
                this.IntelliSensePopup.IsOpen = true;
            }
        }

        public override void SaveDocument(string path)
        {
            this.HasChanges = false;
            this.RaisePropertyChanged("Title");
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var writer = new StreamWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(this.Document.Text);
                writer.Flush();
            }
        }
        public override void OpenDocument(string path)
        {
            this.SFBRef = Workspace.CurrentWorkspace.CurrentSolution.GetOrCreateFileReference(path);
            this.SetFilePath(path);
            if(!File.Exists(path))
            {
                MessageBox.Show(string.Format(Properties.Localization.FileNotFound, path), Properties.Localization.Error, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            using (var stream = File.OpenRead(path))
            {
                this.Document.Text = new StreamReader(stream).ReadToEnd();
                this.Document.UndoStack.ClearAll();
            }
        }
        public override void ReloadDocument()
        {
            this.OpenDocument(this.FilePath);
        }

        protected static IHighlightingDefinition LoadAvalonEditSyntaxFiles(string path)
        {
            if (!File.Exists(path))
                return null;
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var xshd = HighlightingLoader.LoadXshd(XmlReader.Create(stream));
                    var highlightDef = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                    HighlightingManager.Instance.RegisterHighlighting(xshd.Name, xshd.Extensions.ToArray(), highlightDef);
                    return highlightDef;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Localization.XSHD_Loading_Error_Body, path, ex.GetType(), ex.Message), Properties.Localization.XSHD_Loading_Error_Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
        protected virtual IEnumerable<SyntaxError> GetSyntaxErrors()
        {
            return new SyntaxError[0];
        }
        protected virtual IList<IntelliSenseEntry> GetIntelliSenseEntries(TextDocument currentDocument, string currentWord, int caretOffset)
        {
            return new IntelliSenseEntry[0];
            //return new IntelliSenseEntry[] { new IntelliSenseEntry() { Text = "asd" } };
        }
    }
}