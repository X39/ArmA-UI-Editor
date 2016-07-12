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
                CurrentEditingSnap.LastFileConfig.SetKey("onLoad", tb.Text);
                this.CurrentEditingSnap.Redraw();
            }
        }
        private void tb_onLoad_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("onLoad", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsString)
            {
                TextBox tb = sender as TextBox;
                tb.Text = field.String;
            }
        }

        private void tb_ClassName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Ident_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.LastFileConfig.Name = (sender as TextBox).Text;
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_ClassName_Initialized(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Text = this.CurrentEditingSnap.LastFileConfig.Name;
        }

        private void tb_onUnload_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains('\r'))
            {
                TextBox tb = sender as TextBox;
                if (tb.Text.Length == 0)
                    return;
                this.CurrentEditingSnap.LastFileConfig.SetKey("onUnload", tb.Text);
                this.CurrentEditingSnap.Redraw();
            }
        }
        private void tb_onUnload_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("onUnload", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsString)
            {
                TextBox tb = sender as TextBox;
                tb.Text = field.String;
            }
        }

        private void tb_duration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.LastFileConfig.SetKey("duration", (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture));
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_duration_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("duration", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = field.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void tb_fadeIn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.LastFileConfig.SetKey("fadeIn", (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture));
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_fadeIn_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("fadeIn", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = field.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void tb_fadeOut_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.LastFileConfig.SetKey("fadeOut", (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture));
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_fadeOut_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("fadeOut", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = field.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void tb_Idd_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.LastFileConfig.SetKey("idd", (int)double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture));
                this.CurrentEditingSnap.Redraw();
            });
        }
        private void tb_Idd_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("idd", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsNumber)
            {
                TextBox tb = sender as TextBox;
                tb.Text = field.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private void cb_enableSimulation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            this.CurrentEditingSnap.LastFileConfig.SetKey("enableSimulation", bool.Parse((cb.SelectedValue as ComboBoxItem).Content as string) ? 1 : 0);
            this.CurrentEditingSnap.Redraw();
        }
        private void cb_enableSimulation_Initialized(object sender, EventArgs e)
        {
            var field = this.CurrentEditingSnap.LastFileConfig.GetKey("enableSimulation", ConfigField.KeyMode.CheckParentsNull);
            if (field != null && field.IsNumber)
            {
                ComboBox cb = sender as ComboBox;
                cb.SelectedIndex = field.Number > 0 ? 1 : 0;
            }
        }
    }
}
