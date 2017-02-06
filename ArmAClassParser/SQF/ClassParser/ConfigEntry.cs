using System.Windows.Documents;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQF.ClassParser
{
    internal class ConfigEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string callerName = "") { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName)); }

        public ConfigEntry(ConfigEntry parent)
        {
            parent.Children.Add(this);
        }
        private object _Value;
        public object Value { get { return _Value; } set {  this._Value = value; this.RaisePropertyChanged(); } }
        public ObservableCollection<ConfigEntry> Children { get { return (ObservableCollection<ConfigEntry>)_Value; } set { this._Value = value; this.RaisePropertyChanged(); this.RaisePropertyChanged("Value"); } }

        public string Name { get { return new TextRange(this.NameStart, this.NameEnd).Text; } set { new TextRange(this.NameStart, this.NameEnd).Text = value; this.RaisePropertyChanged(); } }
        public string Parent { get { return new TextRange(this.ParentStart, this.ParentEnd).Text; } set { new TextRange(this.ParentStart, this.ParentEnd).Text = value; this.RaisePropertyChanged(); } }
        public string Content { get { return new TextRange(this.ContentStart, this.ContentEnd).Text.FromSqfString(); } set { new TextRange(this.ContentStart, this.ContentEnd).Text = value.ToSqfString(); this.RaisePropertyChanged(); } }


        public TextPointer ContentEnd { get; internal set; }
        public TextPointer ContentStart { get; internal set; }
        public TextPointer NameEnd { get; internal set; }
        public TextPointer NameStart { get; internal set; }
        public TextPointer ParentEnd { get; internal set; }
        public TextPointer ParentStart { get; internal set; }
    }
}