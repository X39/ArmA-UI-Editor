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
            
            this.AddToDockerOrFocus<EditingSnap>();
            this.AddToDockerOrFocus<OutputSnap>();
            this.AddToDockerOrFocus<DialogPropertiesSnap>();
            this.AddToDockerOrFocus<PropertySnap>();
            this.AddToDockerOrFocus<ZIndexSnap>();
            this.AddToDockerOrFocus<ToolboxSnap>();
            this.AddToDockerOrFocus<EventsSnap>();
        }
        public static MainWindow TryGet()
        {
            if (App.Current.MainWindow is MainWindow)
                return App.Current.MainWindow as MainWindow;
            else
                return null;
        }
        #region Docker interaction
        public T NewSnapInstance<T>() where T : Code.Interface.ISnapWindow
        {
            return (T)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
        }
        public void AddToDockerOrFocus<T>() where T : Code.Interface.ISnapWindow
        {
            var instances = this.GetSnapInstances<T>();
            if (instances.Length == 0)
            {
                this.AddToDocker<T>();
            }
            else
            {
                this.Docker.FocusSnap(instances[0]);
            }
        }
        public void AddToDocker<T>() where T : Code.Interface.ISnapWindow
        {
            AddToDocker(this.NewSnapInstance<T>());
        }
        public void AddToDocker<T>(T content) where T : Code.Interface.ISnapWindow
        {
            if (content.AllowedCount == 1 && this.HasSnapInstance<T>())
            {
                throw new ArgumentException("Cannot add snap to window due to snap already existing");
            }
            //ToDo: Save positions of snaps in settings and load them afterwards
            this.Docker.AddSnap(new SnapWindow(content, content.Title), content.DefaultDock);
        }

        public bool HasSnapInstance<T>() where T : Code.Interface.ISnapWindow
        {
            return this.Docker.FindSnaps<T>().Count != 0;
        }
        public T GetSnapInstance<T>() where T : Code.Interface.ISnapWindow
        {
            var list = this.Docker.FindSnaps<T>();
            if (list.Count == 0)
            {
                T t = this.NewSnapInstance<T>();
                list.Add(t);
                AddToDocker(t);
            }
            if (list[0].AllowedCount != 1)
            {
                throw new ArgumentException("Cannot get snaps with AllowdCount > 1 using this function");
            }
            return list[0];
        }
        public T[] GetSnapInstances<T>() where T : Code.Interface.ISnapWindow
        {
            var list = this.Docker.FindSnaps<T>();
            return list.ToArray();
        }
        #endregion

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
            dlg.Filter = "ArmA Class (.cpp)|*.cpp|ArmA Class (.hpp)|*.hpp";
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
            this.AddToDockerOrFocus<ToolboxSnap>();
        }
        private void MenuItem_View_Properties_Click(object sender, RoutedEventArgs e)
        {
            this.AddToDockerOrFocus<PropertySnap>();
        }
        private void MenuItem_View_Output_Click(object sender, RoutedEventArgs e)
        {
            this.AddToDockerOrFocus<OutputSnap>();
        }

        private void MenuItem_View_ZIndex_Click(object sender, RoutedEventArgs e)
        {
            this.AddToDockerOrFocus<ZIndexSnap>();
        }

        private void MenuItem_View_DialogProperties_Click(object sender, RoutedEventArgs e)
        {
            this.AddToDockerOrFocus<DialogPropertiesSnap>();
        }

        private void MenuItem_View_Events_Click(object sender, RoutedEventArgs e)
        {
            this.AddToDockerOrFocus<EventsSnap>();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tmp = new ConfigView();
            tmp.ShowDialog();
        }
    }
}
