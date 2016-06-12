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
using System.IO;
using ArmA_UI_Editor.Code;
using SQF.ClassParser;
using ArmA_UI_Editor.UI.Snaps;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string CurPropertyPath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            var sWindow = new SnapWindow();
            sWindow.Header.Text = "MyUiConfig.cpp";
            sWindow.ContentFrame.Content = new Snaps.EditingWindow();
            frame.Content = sWindow;
            var outWindow = new Snaps.OutputWindow();
            this.OutputWindowFrame.Content = outWindow;
            MainWindow.DisplaySnap(outWindow);

            sWindow = new SnapWindow();
            sWindow.Header.Text = "Properties";
            var el = new Snaps.PropertyWindow();
            sWindow.ContentFrame.Content = el;
            el.LoadSnap();
            this.PropertiesFrame.Content = sWindow;
        }

        private void ListView_Initialized(object sender, EventArgs e)
        {
            ListView lw = sender as ListView;
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void ToolBox_Initialized(object sender, EventArgs e)
        {
            foreach(var addIn in Code.AddInManager.Instance.AddIns)
            {
                foreach(var it in addIn.UIElements)
                {
                    this.ToolBox.Items.Add(it);
                }
            }
        }

        private Point? ToolBoxLastMouseDown;
        private void ToolBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToolBoxLastMouseDown = e.GetPosition(null);
        }

        private void ToolBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ToolBoxLastMouseDown.HasValue)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = ToolBoxLastMouseDown.Value - mousePos;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    ToolBoxLastMouseDown = null;
                    var data = new Code.UI.DragDrop.UiElementsListBoxData((Code.AddInUtil.UIElement)(sender as ListBox).SelectedItem);
                    DragDrop.DoDragDrop(sender as ListBox, new DataObject("UiElementsListBoxData", data), DragDropEffects.Move);
                    e.Handled = true;
                }
            }
        }

        internal static void DisplaySnap(Code.Interface.ISnapWindow snap)
        {
            //ToDo: Implement load mechanism
            snap.LoadSnap();
        }

        private void ConfigRenderCanvas_DragOver(object sender, DragEventArgs e)
        {
        }

        private void ConfigRenderCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent("File"))
            {
                e.Effects = DragDropEffects.None;
            }
        }
        public void SetStatusbarText(string text, bool isError)
        {
            if (isError)
            {
                this.StatusBar.Background = App.Current.Resources["SCB_UIRed"] as SolidColorBrush;
                this.StatusTextbox.Text = text;
            }
            else
            {
                this.StatusBar.Background = App.Current.Resources["SCB_UIBlue"] as SolidColorBrush;
                this.StatusTextbox.Text = text;
            }
        }

        //private void LoadProperties(Code.AddInUtil.Properties properties, Data data)
        //{
        //    this.ElementProperties.Children.Clear();
        //    foreach (var groupIt in properties.Items)
        //    {
        //        var group = new PropertyGroup();
        //        group.IsExpaned = true;
        //        group.Header.Text = groupIt.Name;
        //        this.ElementProperties.Children.Add(group);
        //        foreach (var property in groupIt.Items)
        //        {
        //            var el = new Property();
        //            el.Title.Text = property.DisplayName;
        //            Data d = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data, property.FieldPath);
        //            var fEl = property.PropertyType.GenerateUiElement(d);
        //            el.ConfigElement.Content = fEl;
        //            fEl.Tag = new Code.AddInUtil.Properties.Property.PTypeDataTag { File = this.configFile, Path = property.FieldPath, BaseData = data };
        //            group.ItemsPanel.Children.Add(el);
        //        }
        //    }
        //}
    }
}
