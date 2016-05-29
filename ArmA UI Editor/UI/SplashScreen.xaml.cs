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
            BackgroundWorker worker = ((BackgroundWorker)sender);
            try
            {
                Code.AddInManager.Instance.ReloadAddIns(new Progress<Tuple<double, string>>((t) => {
                    worker.ReportProgress((int)(t.Item1 * 100), t.Item2);
                }));
            }
            catch(Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                do
                {
                    sb.AppendLine(ex.Message);
                    sb.AppendLine(ex.StackTrace);
                    sb.AppendLine("--------------------------------------------------");
                    ex = ex.InnerException;
                } while (ex != null);
                MessageBox.Show(sb.ToString());
            }
            worker.ReportProgress(0, "Checking for Update ...");
#if DEBUG
            var updateResultTask = Code.UpdateManager.Instance.CheckForUpdate(@"http://x39.io/api.php?action=projects&project=ArmA-UI-Editor");
            double d = 0;
            while(!updateResultTask.IsCompleted)
            {
                d += 0.01;
                Thread.Sleep(100);
                worker.ReportProgress((int)(d * 100), "Checking for Update ...");
            }
            var updateResult = updateResultTask.Result;
            if(updateResult.IsAvailable)
            {
                worker.ReportProgress(100, string.Format("Update {0} available", updateResult.NewVersion.ToString()));
                if (MessageBox.Show(string.Format("Update {0} is available for the ArmA-UI-Editor\nDo you want to update now?\n\nChoosing [Yes] will open your webbrowser and download the updated setup.exe.", updateResult.NewVersion.ToString()), "Update Available <3", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    //ToDo: use internal update mechanism and do not rely on browser
                    System.Diagnostics.Process.Start(updateResult.DownloadUrl);
                }
            }
            else
            {
                worker.ReportProgress(100, "No Update available");
            }
#endif
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
