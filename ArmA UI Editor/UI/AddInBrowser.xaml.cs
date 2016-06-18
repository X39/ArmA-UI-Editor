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
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for AddInBrowser.xaml
    /// </summary>
    public partial class AddInBrowser : Window
    {
        [DllImport("shell32.dll")]
        static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);
        private static void openFile(string path)
        {
            var builder = new StringBuilder();
            int res = FindExecutable(path, null, builder);
            switch (res)
            {
                case 31:
                    System.Diagnostics.Process.Start("notepad.exe", path);
                    break;
                case 2:
                case 3:
                case 5:
                case 8:
                    MessageBox.Show("Unable to open " + path + "\r\n" + res, "whoops", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                default:
                    System.Diagnostics.Process.Start(builder.ToString(), path);
                    break;
            }
        }

        public AddInBrowser()
        {
            InitializeComponent();
            this.lb_AddIns.SelectedValuePath = "";
            this.lb_AddIns.DisplayMemberPath = "Info.Name";
            foreach (var it in Code.AddInManager.Instance.AddIns)
            {
                this.lb_AddIns.Items.Add(it);
            }
            if(this.lb_AddIns.Items.Count > 0)
            {
                for (int i = 0; i < this.lb_AddIns.Items.Count; i++)
                {
                    var addin = this.lb_AddIns.Items[i] as Code.AddIn;
                    if(addin.Info.Name.Equals("base", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.lb_AddIns.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void lb_AddIns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.LoadAddin(((sender as ListBox).SelectedValue as Code.AddIn));
        }
        private void LoadAddin(Code.AddIn addin)
        {
            this.sp_AddInContent.Children.Clear();
            UI.PropertyGroup pg;

            pg = new PropertyGroup();
            pg.Header.Text = "Author info";
            this.sp_AddInContent.Children.Add(pg);
            {
                Property p;
                TextBlock tb;

                p = new Property();
                pg.ItemsPanel.Children.Add(p);
                p.Title.Text = "Author";
                tb = new TextBlock(new Run(addin.Info.Author));
                tb.Foreground = App.Current.Resources["SCB_DefaultTextColor"] as SolidColorBrush;
                p.ConfigElement.Content = tb;

                p = new Property();
                pg.ItemsPanel.Children.Add(p);
                p.Title.Text = "Description";
                tb = new TextBlock(new Run(addin.Info.Description));
                tb.Foreground = App.Current.Resources["SCB_DefaultTextColor"] as SolidColorBrush;
                p.ConfigElement.Content = tb;

                p = new Property();
                pg.ItemsPanel.Children.Add(p);
                p.Title.Text = "Version";
                tb = new TextBlock(new Run(addin.Info.Version));
                tb.Foreground = App.Current.Resources["SCB_DefaultTextColor"] as SolidColorBrush;
                p.ConfigElement.Content = tb;
            }

            pg = new PropertyGroup();
            pg.Header.Text = "UI-Elements";
            this.sp_AddInContent.Children.Add(pg);
            foreach (var it in addin.UIElements)
            {
                StackPanel panel = new StackPanel();
                panel.Tag = it;
                panel.Orientation = Orientation.Horizontal;
                Image img = new Image();
                img.Source = new BitmapImage(new Uri(it.Image));
                img.Margin = new Thickness(0, 0, 2, 0);
                panel.Children.Add(img);
                TextBlock tb = new TextBlock();
                tb.Text = it.DisplayName;
                tb.Foreground = App.Current.Resources["SCB_DefaultTextColor"] as SolidColorBrush;
                panel.MouseEnter += (sender, e) =>
                {
                    (sender as StackPanel).Background = App.Current.Resources["SCB_UIBackgroundLight"] as SolidColorBrush;
                };
                panel.MouseLeave += (sender, e) =>
                {
                    (sender as StackPanel).Background = Brushes.Transparent;
                };
                panel.MouseLeftButtonDown += (sender, e) =>
                {
                    if (e.ClickCount == 2)
                    {
                        openFile(it.__ClassPath);
                    }
                };
                panel.MouseRightButtonDown += (sender, e) =>
                {
                    ContextMenu cm = this.FindResource("ContextMenu_UIElements") as ContextMenu;
                    cm.Tag = it;
                    cm.PlacementTarget = sender as UIElement;
                    cm.IsOpen = true;
                };
                panel.Children.Add(tb);
                pg.ItemsPanel.Children.Add(panel);
            }

            pg = new PropertyGroup();
            pg.Header.Text = "Styles";
            this.sp_AddInContent.Children.Add(pg);
            foreach (var it in addin.Styles)
            {
                TextBlock tb = new TextBlock();
                tb.Text = it.Name;
                tb.Foreground = App.Current.Resources["SCB_DefaultTextColor"] as SolidColorBrush;
                tb.MouseEnter += (sender, e) =>
                {
                    (sender as TextBlock).Background = App.Current.Resources["SCB_UIBackgroundLight"] as SolidColorBrush;
                };
                tb.MouseLeave += (sender, e) =>
                {
                    (sender as TextBlock).Background = Brushes.Transparent;
                };
                tb.MouseLeftButtonDown += (sender, e) =>
                {
                    if (e.ClickCount == 2)
                    {
                        it.LoadStyle();
                    }
                };
                pg.ItemsPanel.Children.Add(tb);
            }

        }

        private void ContextMenu_OpenClassFile_Click(object sender, RoutedEventArgs e)
        {
            openFile((((sender as MenuItem).Parent as ContextMenu).Tag as Code.AddInUtil.UIElement).__ClassPath);
        }
        private void ContextMenu_OpenXamlFile_Click(object sender, RoutedEventArgs e)
        {
            openFile((((sender as MenuItem).Parent as ContextMenu).Tag as Code.AddInUtil.UIElement).__XamlPath);
        }
        private void ContextMenu_OpenPropertiesFile_Click(object sender, RoutedEventArgs e)
        {
            openFile((((sender as MenuItem).Parent as ContextMenu).Tag as Code.AddInUtil.UIElement).__PropertiesPath);
        }
    }
}
