using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.ComponentModel;

namespace ArmA.Studio.Dialogs
{
    public class WorkspaceSelectorDialogDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string callerName = "") { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName)); }

        public string CurrentPath { get { return this._CurrentPath; } set { this._CurrentPath = value; this.RaisePropertyChanged(); } }
        private string _CurrentPath;
        public ICommand CmdBrowse { get; private set; }
        public ICommand CmdOK { get; private set; }
        public bool? DialogResult { get { return this._DialogResult; } set { this._DialogResult = value; this.RaisePropertyChanged(); } }
        private bool? _DialogResult;
        public WorkspaceSelectorDialogDataContext()
        {
            this.CmdBrowse = new UI.Commands.RelayCommand(Cmd_Browse);
            this.CmdOK = new UI.Commands.RelayCommand(Cmd_OK);
        }
        public void Cmd_Browse(object param)
        {
            var fbdlg = new FolderBrowserDialog();
            fbdlg.ShowNewFolderButton = true;
            fbdlg.Description = Properties.Localization.WorkspaceSelectorDialog_Description;
            if (!string.IsNullOrWhiteSpace(this.CurrentPath))
            {
                fbdlg.SelectedPath = CurrentPath;
            }
            var dlgResult = fbdlg.ShowDialog();
            if(dlgResult == System.Windows.Forms.DialogResult.OK)
            {
                this.CurrentPath = fbdlg.SelectedPath;
            }
        }
        public void Cmd_OK(object param)
        {
            if(string.IsNullOrWhiteSpace(CurrentPath))
            {
                MessageBox.Show(Properties.Localization.WorkspaceSelectorDialog_NoWorkspaceSelected, Properties.Localization.Whoops, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.DialogResult = true;
            }
        }
    }
}
