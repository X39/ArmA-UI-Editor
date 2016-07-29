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

        private void tb_enterConfirm_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                (sender as DependencyObject).ForceBindingSourceUpdate(TextBox.TextProperty);
            }
        }

        private void tb_ClassName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Ident_DoHandle(sender, e, () =>
            {
                this.CurrentEditingSnap.RenameConfigKey(this.CurrentEditingSnap.LastFileConfig.Key, (sender as TextBox).Text);
                this.CurrentEditingSnap.LastFileConfig.Name = (sender as TextBox).Text;
                this.CurrentEditingSnap.RegenerateDisplay();
            });
        }

        private void tb_ClassName_Initialized(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Text = this.CurrentEditingSnap.LastFileConfig.Name;
        }

        private void tb_StringValueBinding_Initialized(object sender, EventArgs e)
        {
            var el = sender as TextBox;
            var binding = new Binding("Value");
            binding.Source = AddInManager.Instance.MainFile;
            binding.NotifyOnSourceUpdated = true;

            binding.Converter = new Code.AddInUtil.PropertyUtil.StringType.NormalPropertyConverter(string.Join("/", this.CurrentEditingSnap.LastFileConfig.Key, el.Tag as string));
            el.SetBinding(TextBox.TextProperty, binding);
        }
        private void tb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.CurrentEditingSnap.UpdateConfigKey(((sender as FrameworkElement).GetBinding(TextBox.TextProperty).Converter as Code.Converter.ConfigFieldKeyConverterBase).Key);
        }
        private void cb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.CurrentEditingSnap.UpdateConfigKey(((sender as FrameworkElement).GetBinding(ComboBox.SelectedIndexProperty).Converter as Code.Converter.ConfigFieldKeyConverterBase).Key);
        }

        private void tb_NumberValueBinding_Initialized(object sender, EventArgs e)
        {
            var el = sender as TextBox;
            var binding = new Binding("Value");
            binding.Source = AddInManager.Instance.MainFile;
            binding.NotifyOnSourceUpdated = true;

            binding.Converter = new Code.AddInUtil.PropertyUtil.NumberType.NormalPropertyConverter(string.Join("/", this.CurrentEditingSnap.LastFileConfig.Key, el.Tag as string));
            binding.ConverterParameter = new Code.AddInUtil.PropertyUtil.NumberType.ConverterPropertyData { Conversion = string.Empty, Window = this.CurrentEditingSnap };
            el.SetBinding(TextBox.TextProperty, binding);
        }
        private void tb_NumberValueBinding_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () => {});
        }

        private void cb_BoolValueBinding_Initialized(object sender, EventArgs e)
        {
            var el = sender as ComboBox;
            var binding = new Binding("Value");
            binding.Source = AddInManager.Instance.MainFile;
            binding.NotifyOnSourceUpdated = true;

            binding.Converter = new Code.AddInUtil.PropertyUtil.BooleanType.NormalPropertyConverter(string.Join("/", this.CurrentEditingSnap.LastFileConfig.Key, el.Tag as string));
            el.SetBinding(ComboBox.SelectedIndexProperty, binding);
        }
    }
}
