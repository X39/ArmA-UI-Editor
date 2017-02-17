using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;

namespace ArmA.Studio.Dialogs
{
    public class LicenseViewerDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string callerName = "") { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName)); }

        public ObservableCollection<License> Licenses { get; set; }
        public ICommand CmdOK { get; private set; }
        public bool? DialogResult { get { return this._DialogResult; } set { this._DialogResult = value; this.RaisePropertyChanged(); } }
        private bool? _DialogResult;
        public LicenseViewerDataContext()
        {
            this.CmdOK = new UI.Commands.RelayCommand(Cmd_OK);
            var arr = App.Current.TryFindResource("Licenses") as Array;
            this.Licenses = new ObservableCollection<License>(arr.Cast<License>());
        }
        public void Cmd_OK(object param)
        {
            this.DialogResult = true;
        }
    }
}
