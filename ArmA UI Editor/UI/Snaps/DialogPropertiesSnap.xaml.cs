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
using System.Windows.Markup;
using ArmA_UI_Editor.Code;


namespace ArmA_UI_Editor.UI.Snaps
{
    public partial class DialogPropertiesSnap : Page, Code.Interface.ISnapWindow
    {
        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Right; } }

        private EditingSnap _CurrentEditingSnap;
        private EditingSnap CurrentEditingSnap
        {
            get
            {
                return this._CurrentEditingSnap;
            }
            set
            {
                this._CurrentEditingSnap = value;
                if (value != null)
                {
                    ThisFrame.Content = this.Resources["WindowProperties"];
                }
            }
        }

        public DialogPropertiesSnap()
        {
            InitializeComponent();
            _CurrentEditingSnap = null;
        }
        public void UnloadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange -= Docker_OnSnapFocusChange;
        }

        public void LoadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange += Docker_OnSnapFocusChange;
            var EditingSnaps = ArmA_UI_Editor.UI.MainWindow.TryGet().Docker.FindSnaps<EditingSnap>(true);
            if (EditingSnaps.Count > 0)
            {
                this.CurrentEditingSnap = EditingSnaps[0];
            }
        }

        private void Docker_OnSnapFocusChange(object sender, SnapDocker.OnSnapFocusChangeEventArgs e)
        {
            if(e.SnapWindowNew == null)
            {
                if(e.SnapWindowLast != null && e.SnapWindowLast.Window == this.CurrentEditingSnap)
                {
                    this.CurrentEditingSnap = null;
                }
            }
            else if(e.SnapWindowNew.Window is EditingSnap)
            {
                this.CurrentEditingSnap = e.SnapWindowNew.Window as EditingSnap;
            }
        }

        private void tb_onLoad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains('\r'))
            {
                TextBox tb = sender as TextBox;
                if (tb.Text.Length == 0)
                    return;
                var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/onLoad", true);
                data.String = tb.Text;
                this.CurrentEditingSnap.Redraw();
            }
        }
        private void tb_onLoad_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/onLoad", false);
            if (data != null && data.IsString)
            {
                TextBox tb = sender as TextBox;
                tb.Text = data.String;
            }
        }

        private void tb_ClassName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Ident_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1].Name = (sender as TextBox).Text;
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_ClassName_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            TextBox tb = sender as TextBox;
            tb.Text = uiConfigClass.Name;
        }

        private void tb_onUnload_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains('\r'))
            {
                TextBox tb = sender as TextBox;
                if (tb.Text.Length == 0)
                    return;
                var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/onUnload", true);
                data.String = tb.Text;
                this.CurrentEditingSnap.Redraw();
            }
        }
        private void tb_onUnload_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/onUnload", false);
            if (data != null && data.IsString)
            {
                TextBox tb = sender as TextBox;
                tb.Text = data.String;
            }
        }

        private void tb_duration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/duration", true);
                data.Number = (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture);
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_duration_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/duration", false);
            if (data != null && data.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = data.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void tb_fadeIn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/fadeIn", true);
                data.Number = (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture);
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_fadeIn_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/fadeIn", false);
            if (data != null && data.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = data.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void tb_fadeOut_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/fadeOut", true);
                data.Number = (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture);
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_fadeOut_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/fadeOut", false);
            if (data != null && data.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = data.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void tb_Idd_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/idd", true);
                data.Number = (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture);
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_Idd_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/idd", false);
            if (data != null && data.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = data.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void cb_enableSimulation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            int value = bool.Parse((cb.SelectedValue as ComboBoxItem).Content as string) ? 1 : 0;
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/enableSimulation", true);
            data.Number = value;
            this.CurrentEditingSnap.Redraw();
        }
        private void cb_enableSimulation_Initialized(object sender, EventArgs e)
        {
            var uiConfigClass = this.CurrentEditingSnap.ConfigFile[this.CurrentEditingSnap.ConfigFile.Count - 1];
            var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(uiConfigClass, "/enableSimulation", false);
            if (data != null && data.IsNumber)
            {
                ComboBox cb = sender as ComboBox;
                cb.SelectedIndex = data.Number > 0 ? 1 : 0;
            }
        }
    }
}
