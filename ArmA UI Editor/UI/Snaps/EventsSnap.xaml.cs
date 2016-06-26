
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

        public Data CurrentData { get; private set; }

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
                LoadEvents(e.Tags[0].file, e.Tags[0].data);
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

        public void LoadEvents(ArmA_UI_Editor.Code.AddInUtil.UIElement element, SQF.ClassParser.Data data)
        {
            this.EventStackPanel.Children.Clear();
            this.CurrentData = data;
            foreach (var e in element.Events)
            {
                var el = new Property();
                this.EventStackPanel.Children.Add(el);

                el.Header = e.Name;
                var tb = new TextBox();
                el.Children.Add(tb);

                var d = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data, e.Field, false);
                if(d != null && d.IsString)
                    tb.Text = d.String;
                tb.PreviewTextInput += Tb_PreviewTextInput;
                tb.Tag = e;
            }
        }

        private void Tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(e.Text.Contains('\r'))
            {
                Event ev = (Event)(sender as FrameworkElement).Tag;
                var d = SQF.ClassParser.File.ReceiveFieldFromHirarchy(CurrentData, ev.Field, true);
                d.String = (sender as TextBox).Text;
                this.CurrentEditingSnap.Redraw();
                e.Handled = true;
            }
        }
    }
}
