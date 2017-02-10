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
    public class ConfigEntrySelectorDialogDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string callerName = "") { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName)); }

        public ObservableCollection<object> ThisCollection { get { return this._ThisCollection; } set { this._ThisCollection = value; this.RaisePropertyChanged(); } }
        private ObservableCollection<object> _ThisCollection;
        public object SelectedValue { get { return this._SelectedValue; } set { this._SelectedValue = value; this.RaisePropertyChanged(); } }
        private object _SelectedValue;
        public ICommand CmdOK { get; private set; }
        public bool? DialogResult { get { return this._DialogResult; } set { this._DialogResult = value; this.RaisePropertyChanged(); } }
        private bool? _DialogResult;
        public ConfigEntrySelectorDialogDataContext()
        {
            this.CmdOK = new UI.Commands.RelayCommand(Cmd_OK);
            this._ThisCollection = new ObservableCollection<object>();
        }
        public void Cmd_OK(object param)
        {
            this.DialogResult = true;
        }
    }
}
