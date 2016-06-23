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
            Property p = new Property();
            TextBox tb = new TextBox();
            tb.PreviewTextInput += TextBox_ClassName_PreviewTextInput;
            tb.Text = this.CurrentData.Name;
            p.Children.Add(tb);
            p.Header = "Class Name";
            group.Children.Add(p);
            this.PropertyStack.Children.Add(group);
        }
        public void LoadProperties(Code.AddInUtil.Properties properties, SQF.ClassParser.Data data, EditingSnap window)
        {
            this.CurrentProperties = properties;
            this.CurrentData = data;
            this.CurrentWindow = window;

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
                    var fEl = property.PropertyType.GenerateUiElement(d, window);
                    el.Children.Add(fEl);
                    fEl.Tag = new Code.AddInUtil.Properties.Property.PTypeDataTag { File = window.ConfigFile, Path = property.FieldPath, BaseData = data };
                    group.ItemsPanel.Children.Add(el);
                }
            }
        }

        public void UnloadSnap()
        {
            Code.AddInUtil.Properties.Property.PType.ValueChanged -= PType_ValueChanged;
            Code.AddInUtil.Properties.Property.PType.OnError -= PType_OnError;
        }


        public void LoadSnap()
        {
            Code.AddInUtil.Properties.Property.PType.ValueChanged += PType_ValueChanged;
            Code.AddInUtil.Properties.Property.PType.OnError += PType_OnError;
        }

        private void TextBox_ClassName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(e.Text.Contains('\r'))
            {
                this.CurrentData.Name = (sender as TextBox).Text.Trim(new[] { ' ' });
                CurrentWindow.TryRefreshAll(1);
            }
        }
    }
}
