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
    public partial class PropertyWindow : Page, Code.Interface.ISnapWindow
    {
        private static PropertyWindow _Instance;
        public Data CurrentData { get; private set; }
        public Code.AddInUtil.Properties CurrentProperties { get; private set; }
        public EditingWindow CurrentWindow { get; private set; }

        public PropertyWindow()
        {
            InitializeComponent();
            Code.AddInUtil.Properties.Property.PType.ValueChanged += PType_ValueChanged;
        }

        private void PType_ValueChanged(object sender, EventArgs e)
        {
            CurrentWindow.Redraw();
        }

        ~PropertyWindow()
        {

        }
        public void LoadProperties(Code.AddInUtil.Properties properties, SQF.ClassParser.Data data, EditingWindow window)
        {
            this.CurrentProperties = properties;
            this.CurrentData = data;
            this.CurrentWindow = window;

            this.PropertyStack.Children.Clear();
            foreach (var groupIt in properties.Items)
            {
                var group = new PropertyGroup();
                group.IsExpaned = true;
                group.Header.Text = groupIt.Name;
                this.PropertyStack.Children.Add(group);
                foreach (var property in groupIt.Items)
                {
                    var el = new Property();
                    el.Title.Text = property.DisplayName;
                    Data d = File.ReceiveFieldFromHirarchy(data, property.FieldPath);
                    var fEl = property.PropertyType.GenerateUiElement(d);
                    el.ConfigElement.Content = fEl;
                    fEl.Tag = new Code.AddInUtil.Properties.Property.PTypeDataTag { File = window.ConfigFile, Path = property.FieldPath, BaseData = data };
                    group.ItemsPanel.Children.Add(el);
                }
            }
        }


        internal static PropertyWindow GetDisplayWindow()
        {
            if(_Instance == null)
            {
                MainWindow.DisplaySnap(new PropertyWindow());
            }
            return _Instance;
        }

        public void UnloadSnap()
        {
            _Instance = null;
        }

        public void LoadSnap()
        {
            _Instance = this;
        }
    }
}
