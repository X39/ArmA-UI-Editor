
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArmA_UI_Editor.Code.AddInUtil;
using SQF.ClassParser;
using ArmA_UI_Editor.Code;

namespace ArmA_UI_Editor.UI.Snaps
{
    public partial class EventsSnap : Page, Code.Interface.ISnapWindow
    {
        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Right; } }

        public string CurrentKey { get; private set; }

        private EditingSnap CurrentEditingSnap;

        public EventsSnap()
        {
            InitializeComponent();
        }
        public void UnloadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange -= Docker_OnSnapFocusChange;
            if (CurrentEditingSnap != null)
                CurrentEditingSnap.OnSelectedFocusChanged -= CurrentEditingSnap_OnSelectedFocusChanged;
        }
        public void LoadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange += Docker_OnSnapFocusChange;
            var EditingSnaps = (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.FindSnaps<EditingSnap>(true);
            if (EditingSnaps.Count > 0)
            {
                CurrentEditingSnap = EditingSnaps[0];
                CurrentEditingSnap.OnSelectedFocusChanged += CurrentEditingSnap_OnSelectedFocusChanged;
            }
        }

        private void CurrentEditingSnap_OnSelectedFocusChanged(object sender, EditingSnap.OnSelectedFocusChangedEventArgs e)
        {
            if (e.Tags.Length != 1)
            {
                this.EventStackPanel.Children.Clear();
            }
            else
            {
                LoadEvents(e.Tags[0].file, e.Tags[0].Key);
            }
        }

        private void Docker_OnSnapFocusChange(object sender, SnapDocker.OnSnapFocusChangeEventArgs e)
        {
            if (e.SnapWindowNew != null && e.SnapWindowNew.Window is EditingSnap)
            {
                if (CurrentEditingSnap != null)
                    CurrentEditingSnap.OnSelectedFocusChanged -= CurrentEditingSnap_OnSelectedFocusChanged;
                CurrentEditingSnap = e.SnapWindowNew.Window as EditingSnap;
                CurrentEditingSnap.OnSelectedFocusChanged += CurrentEditingSnap_OnSelectedFocusChanged;
            }
            else if (e.SnapWindowLast != null && e.SnapWindowLast.Window is EditingSnap)
            {
                if (CurrentEditingSnap != null)
                    CurrentEditingSnap.OnSelectedFocusChanged -= CurrentEditingSnap_OnSelectedFocusChanged;
                CurrentEditingSnap = null;
            }
        }

        public void LoadEvents(ArmA_UI_Editor.Code.AddInUtil.UIElement element, string key)
        {
            this.EventStackPanel.Children.Clear();
            this.CurrentKey = key;
            foreach (var e in element.Events)
            {
                var el = new Property();
                this.EventStackPanel.Children.Add(el);

                el.Header = e.Name;
                var tb = new TextBox();
                el.Children.Add(tb);
                var field = AddInManager.Instance.MainFile.GetKey(string.Join("/", this.CurrentKey, e.Field), ConfigField.KeyMode.NullOnNotFound);

                if (field != null && field.IsString)
                {
                    if(string.IsNullOrWhiteSpace(e.StartingAt))
                    {
                        tb.Text = field.String;
                    }
                    else
                    {
                        var index = field.String.IndexOf(e.StartingAt);
                        if (index >= 0)
                            tb.Text = field.String.Substring(index + e.StartingAt.Length);
                        else
                            tb.Text = "";
                    }
                }
                tb.PreviewTextInput += Tb_PreviewTextInput;
                tb.Tag = e;
            }
        }

        private void Tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(e.Text.Contains('\r'))
            {
                Event ev = (Event)(sender as FrameworkElement).Tag;
                var field = AddInManager.Instance.MainFile[string.Join("/", this.CurrentKey, ev.Field)];
                if (string.IsNullOrWhiteSpace(ev.StartingAt))
                {
                    field.Parent.SetKey(field.Name, (sender as TextBox).Text);
                }
                else
                {
                    var index = field.IsString ? field.String.IndexOf(ev.StartingAt) : -1;
                    if (index >= 0)
                        field.Parent.SetKey(field.Name, field.String.Substring(0, index) + ev.StartingAt + (sender as TextBox).Text);
                    else
                        field.Parent.SetKey(field.Name, ev.StartingAt + (sender as TextBox).Text);
                }
                this.CurrentEditingSnap.Redraw();
                e.Handled = true;
            }
        }
    }
}
