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
    /// <summary>
    /// Interaction logic for PropertyWindow.xaml
    /// </summary>
    public partial class PropertySnap : Page, Code.Interface.ISnapWindow
    {
        public Data CurrentData { get; private set; }
        public Code.AddInUtil.Properties CurrentProperties { get; private set; }
        public EditingSnap CurrentWindow { get; private set; }

        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Right; } }

        public PropertySnap()
        {
            InitializeComponent();
        }

        private void PType_ValueChanged(object sender, EventArgs e)
        {
            CurrentWindow.Redraw();
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).SetStatusbarText("", false);
        }
        private void PType_OnError(object sender, string e)
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).SetStatusbarText(e, true);
        }

        private void AddDefaultProperties()
        {
            var group = new Group();
            group.Header = "Default";
            this.PropertyStack.Children.Add(group);
            Property p;
            TextBox tb;
            Data d;

            p = new Property();
            tb = new TextBox();
            tb.PreviewTextInput += TextBox_ClassName_PreviewTextInput;
            tb.Text = this.CurrentData.Name;
            p.Children.Add(tb);
            p.Header = "Class Name";
            group.Children.Add(p);

            p = new Property();
            tb = new TextBox();
            tb.PreviewTextInput += TextBox_IDC_PreviewTextInput;
            d = SQF.ClassParser.File.ReceiveFieldFromHirarchy(this.CurrentData, "idc", false);
            if(d != null && d.IsNumber)
            {
                tb.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", d.Number);
            }
            p.Children.Add(tb);
            p.Header = "IDC";
            group.Children.Add(p);
        }

        private void TextBox_IDC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                SQF.ClassParser.File.ReceiveFieldFromHirarchy(this.CurrentData, "idc", true).Number = double.Parse((sender as TextBox).Text, System.Globalization.CultureInfo.InvariantCulture);
                CurrentWindow.Redraw();
                (ArmA_UI_Editor.UI.MainWindow.TryGet()).SetStatusbarText("", false);
            });
        }
        private void TextBox_ClassName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Ident_DoHandle(sender, e, () =>
            {
                this.CurrentData.Name = (sender as TextBox).Text;
                CurrentWindow.TryRefreshAll(1);
            });
        }

        public void LoadProperties(Code.AddInUtil.Properties properties, SQF.ClassParser.Data data)
        {
            this.CurrentProperties = properties;
            this.CurrentData = data;
            this.PropertyStack.Children.Clear();
            this.AddDefaultProperties();

            foreach (var groupIt in properties.Items)
            {
                var group = new Group();
                group.IsExpaned = true;
                group.Header = groupIt.Name;
                this.PropertyStack.Children.Add(group);
                foreach (var property in groupIt.Items)
                {
                    var el = new Property();
                    el.Header = property.DisplayName;
                    Data d = File.ReceiveFieldFromHirarchy(data, property.FieldPath);
                    var fEl = property.PropertyType.GenerateUiElement(d, CurrentWindow);
                    el.Children.Add(fEl);
                    fEl.Tag = new Code.AddInUtil.Properties.Property.PTypeDataTag { File = CurrentWindow.ConfigFile, Path = property.FieldPath, BaseData = data };
                    group.ItemsPanel.Children.Add(el);
                }
            }
        }

        public void UnloadSnap()
        {
            Code.AddInUtil.Properties.Property.PType.ValueChanged -= PType_ValueChanged;
            Code.AddInUtil.Properties.Property.PType.OnError -= PType_OnError;
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange -= Docker_OnSnapFocusChange;
            if(CurrentWindow != null)
                CurrentWindow.OnSelectedFocusChanged -= CurrentWindow_OnSelectedFocusChanged;

        }
        public void LoadSnap()
        {
            Code.AddInUtil.Properties.Property.PType.ValueChanged += PType_ValueChanged;
            Code.AddInUtil.Properties.Property.PType.OnError += PType_OnError;
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange += Docker_OnSnapFocusChange;
            var EditingSnaps = (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.FindSnaps<EditingSnap>(true);
            if (EditingSnaps.Count > 0)
            {
                CurrentWindow = EditingSnaps[0];
                CurrentWindow.OnSelectedFocusChanged += CurrentWindow_OnSelectedFocusChanged;
            }
        }

        private void CurrentWindow_OnSelectedFocusChanged(object sender, EditingSnap.OnSelectedFocusChangedEventArgs e)
        {
            if(e.Tags.Length != 1)
            {
                this.PropertyStack.Children.Clear();
            }
            else
            {
                LoadProperties(e.Tags[0].file.Properties, e.Tags[0].data);
            }
        }

        private void Docker_OnSnapFocusChange(object sender, SnapDocker.OnSnapFocusChangeEventArgs e)
        {
            if (e.SnapWindowNew != null && e.SnapWindowNew.Window is EditingSnap)
            {
                this.PropertyStack.Children.Clear();
                if(CurrentWindow != null)
                    CurrentWindow.OnSelectedFocusChanged -= CurrentWindow_OnSelectedFocusChanged;
                CurrentWindow = e.SnapWindowNew.Window as EditingSnap;
                CurrentWindow.OnSelectedFocusChanged += CurrentWindow_OnSelectedFocusChanged;
            }
            else if (e.SnapWindowLast != null && e.SnapWindowLast.Window is EditingSnap)
            {
                if (e.SnapWindowLast.Window != CurrentWindow)
                    return;
                if (CurrentWindow != null)
                    CurrentWindow.OnSelectedFocusChanged -= CurrentWindow_OnSelectedFocusChanged;
                this.PropertyStack.Children.Clear();
                CurrentWindow = null;
            }
        }
    }
}
