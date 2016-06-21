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
        private static PropertySnap _Instance;
        public Data CurrentData { get; private set; }
        public Code.AddInUtil.Properties CurrentProperties { get; private set; }
        public EditingSnap CurrentWindow { get; private set; }

        public PropertySnap()
        {
            InitializeComponent();
        }

        private void PType_ValueChanged(object sender, EventArgs e)
        {
            CurrentWindow.Redraw();
            (App.Current.MainWindow as MainWindow).SetStatusbarText("", false);
        }
        private void PType_OnError(object sender, string e)
        {
            (App.Current.MainWindow as MainWindow).SetStatusbarText(e, true);
        }

        ~PropertySnap()
        {

        }
        public void LoadProperties(Code.AddInUtil.Properties properties, SQF.ClassParser.Data data, EditingSnap window)
        {
            this.CurrentProperties = properties;
            this.CurrentData = data;
            this.CurrentWindow = window;

            this.PropertyStack.Children.Clear();
            foreach (var groupIt in properties.Items)
            {
                var group = new Group();
                group.IsExpaned = true;
                group.Header = groupIt.Name;
                this.PropertyStack.Children.Add(group);
                foreach (var property in groupIt.Items)
                {
                    var el = new Property();
                    el.Title.Text = property.DisplayName;
                    Data d = File.ReceiveFieldFromHirarchy(data, property.FieldPath);
                    var fEl = property.PropertyType.GenerateUiElement(d, window);
                    el.ConfigElement.Content = fEl;
                    fEl.Tag = new Code.AddInUtil.Properties.Property.PTypeDataTag { File = window.ConfigFile, Path = property.FieldPath, BaseData = data };
                    group.ItemsPanel.Children.Add(el);
                }
            }
        }


        internal static PropertySnap GetDisplayWindow()
        {
            if (_Instance == null)
            {
                (App.Current.MainWindow as MainWindow).Docker.AddSnap(new SnapWindow(new PropertySnap(), App.Current.Resources["STR_Window_Properties"] as string), Dock.Right);
            }
            return _Instance;
        }
        internal static bool HasDisplayWindow()
        {
            return _Instance != null;
        }

        public void UnloadSnap()
        {
            Code.AddInUtil.Properties.Property.PType.ValueChanged -= PType_ValueChanged;
            Code.AddInUtil.Properties.Property.PType.OnError -= PType_OnError;
            _Instance = null;
        }


        public void LoadSnap()
        {
            _Instance = this;
            Code.AddInUtil.Properties.Property.PType.ValueChanged += PType_ValueChanged;
            Code.AddInUtil.Properties.Property.PType.OnError += PType_OnError;
        }
    }
}
