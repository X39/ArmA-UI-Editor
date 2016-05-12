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
using System.Threading;

namespace ArmA_UI_Editor
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private bool isFinished = false;
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            this.VersionLabel.Text = string.Format("Version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            LoadData();
        }
        private async void LoadData()
        {
            await Task.Delay(5000);
            isFinished = true;
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (isFinished)
            {
                App.Current.MainWindow = new MainWindow();
                App.Current.MainWindow.Show();
            }
        }
    }
}
