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
    public class CreateNewFileDialogDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string callerName = "") { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName)); }
        public ICommand CmdOK { get { return new UI.Commands.RelayCommand((p) => this.DialogResult = true); } }
        public bool? DialogResult { get { return this._DialogResult; } set { this._DialogResult = value; this.RaisePropertyChanged(); } }
        private bool? _DialogResult;
        public string SelectedName { get { return this._SelectedName; } set { this._SelectedName = value; this.RaisePropertyChanged(); } }
        private string _SelectedName;
        public ObservableCollection<FileType> FileTypeCollection { get; private set; }
        public object SelectedItem { get { return this._SelectedItem; } set { this._SelectedItem = value; this.RaisePropertyChanged(); } }
        private object _SelectedItem;

        public CreateNewFileDialogDataContext()
        {
            var arr = App.Current.TryFindResource("FileTypes") as Array;
            this.FileTypeCollection = new ObservableCollection<FileType>(arr.Cast<FileType>());
            this.SelectedItem = this.FileTypeCollection.First();
        }


    }
}
