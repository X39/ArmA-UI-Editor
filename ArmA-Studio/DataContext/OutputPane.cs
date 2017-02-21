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
using ArmA.Studio.UI.Commands;

namespace ArmA.Studio.DataContext
{
    public class OutputPane : PanelBase
    {
        public TextDocument Document { get { return this._Document; } set { this._Document = value; this.RaisePropertyChanged(); } }
        private TextDocument _Document;

        public ICommand CmdClearOutputWindow { get; private set; }

        public override string Title { get { return Properties.Localization.PanelDisplayName_Output; } }


        public OutputPane()
        {
            this.Document = new TextDocument();
            this.CmdClearOutputWindow = new RelayCommand((p) => this.Document.Text = string.Empty);

            App.SubscribableLoggerTarget.OnLog += Logger_OnLog;
        }

        private void Logger_OnLog(object sender, LoggerTargets.SubscribableTarget.OnLogEventArgs e)
        {
            this.Document.Insert(this.Document.TextLength, e.Message);
            this.Document.Insert(this.Document.TextLength, "\r\n");
        }
    }
}