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
        public SnapDocker Docker;
        public MainWindow()
        {
            InitializeComponent();
            this.Docker = new SnapDocker();
            this.frame.Content = this.Docker;

            this.Docker.AddSnap(new SnapWindow(new Snaps.EditingSnap(), "MyUiConfig.cpp"), Dock.Top);
            this.Docker.AddSnap(new SnapWindow(new Snaps.OutputSnap(), App.Current.Resources["STR_Window_Output"] as string), Dock.Bottom);
            this.Docker.AddSnap(new SnapWindow(new Snaps.PropertySnap(), App.Current.Resources["STR_Window_Properties"] as string), Dock.Right);
            this.Docker.AddSnap(new SnapWindow(new Snaps.ToolboxSnap(), App.Current.Resources["STR_Window_Toolbox"] as string), Dock.Left);
            this.Docker.AddSnap(new SnapWindow(new Snaps.ZIndexSnap(), App.Current.Resources["STR_Window_ZIndex"] as string), Dock.Left);
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

        private void MenuItem_Settings_ShowAddInBrowser_Click(object sender, RoutedEventArgs e)
        {
            var tmp = new AddInBrowser();
            tmp.Show();
        }

        private void MenuItem_Donate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("As this is currently in alpha, i do not accept donations for this 'Product'.\nIf you still want to donate some $$$ then please head over to the forums and write me a PN.\n\nKindly regards, X39", "Not in Alpha", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Help_OpenDocumentation_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/X39/ArmA-UI-Editor/wiki");
        }

        private void MenuItem_Help_ReportBug_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/X39/ArmA-UI-Editor/issues/new");
        }

        private void MenuItem_Settings_ShowSettingsWindow_Click(object sender, RoutedEventArgs e)
        {
            var tmp = new SettingsViewer();
            tmp.Show();
        }

        private void MenuItem_File_New_Click(object sender, RoutedEventArgs e)
        {
            this.Docker.AddSnap(new SnapWindow(new Snaps.EditingSnap(), "MyUiConfig.cpp"), Dock.Top);
        }

        private void MenuItem_File_Open_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cpp";
            dlg.Filter = "ArmA Class (.cpp)|*.cpp";
            dlg.Filter = "ArmA Class (.hpp)|*.hpp";
            dlg.CheckPathExists = true;
            var res = dlg.ShowDialog();
            if (!res.HasValue || !res.Value)
            {
                return;
            }

            this.Docker.AddSnap(new SnapWindow(new Snaps.EditingSnap(dlg.FileName), System.IO.Path.GetFileName(dlg.FileName)), Dock.Top);
        }

        private void MenuItem_File_Save_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuItem_View_Toolbox_Click(object sender, RoutedEventArgs e)
        {
            var elList = Docker.FindSnaps<Snaps.ToolboxSnap>();
            if (elList.Count == 0)
            {
                this.Docker.AddSnap(new SnapWindow(new Snaps.ToolboxSnap(), App.Current.Resources["STR_Window_Toolbox"] as string), Dock.Left);
            }
        }
        private void MenuItem_View_Properties_Click(object sender, RoutedEventArgs e)
        {
            var elList = Docker.FindSnaps<Snaps.PropertySnap>();
            if (elList.Count == 0)
            {
                this.Docker.AddSnap(new SnapWindow(new Snaps.PropertySnap(), App.Current.Resources["STR_Window_Properties"] as string), Dock.Right);
            }
        }
        private void MenuItem_View_Output_Click(object sender, RoutedEventArgs e)
        {
            var elList = Docker.FindSnaps<Snaps.OutputSnap>();
            if(elList.Count == 0)
            {
                this.Docker.AddSnap(new SnapWindow(new Snaps.OutputSnap(), App.Current.Resources["STR_Window_Output"] as string), Dock.Bottom);
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
