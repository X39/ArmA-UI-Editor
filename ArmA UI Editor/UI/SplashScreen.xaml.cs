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
using System.ComponentModel;

namespace ArmA_UI_Editor.UI
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
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            isFinished = true;
            this.Close();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress_Bar.Value = e.ProgressPercentage / 100;
            Progress_Text.Text = e.UserState as string;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Code.AddInManager.Instance.ReloadAddIns(new Progress<Tuple<double, string>>((t) => {
                    ((BackgroundWorker)sender).ReportProgress((int)(t.Item1 * 100), t.Item2);
                }));
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + '\n' + ex.StackTrace);
            }
            Thread.Sleep(1000);
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
