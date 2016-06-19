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
        private bool doShutdown = false;
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
            Progress_Bar.Value = e.ProgressPercentage / (double)100;
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
            /////////////////////////////////////////////////////
            //Settings.Instance is allowed to be used from here//
            /////////////////////////////////////////////////////
#if !DEBUG
            if (Settings.Instance.SearchUpdateOnStart)
            {
                worker.ReportProgress(0, App.Current.Resources["STR_CODE_SplashScreen_CheckingForUpdate"] as String);
                var updateResultTask = Code.UpdateManager.Instance.CheckForUpdate(@"http://x39.io/api.php?action=projects&project=ArmA-UI-Editor");
                double d = 0;
                while (!updateResultTask.IsCompleted)
                {
                    d += 0.01;
                    Thread.Sleep(100);
                    worker.ReportProgress((int)(d * 100), App.Current.Resources["STR_CODE_SplashScreen_CheckingForUpdate"] as String);
                }
                var updateResult = updateResultTask.Result;
                if (updateResult.IsAvailable)
                {
                    worker.ReportProgress(100, string.Format(App.Current.Resources["STR_CODE_SplashScreen_UpdateAvailableMessage"] as String, updateResult.NewVersion.ToString()));
                    if (MessageBox.Show(string.Format(App.Current.Resources["STR_CODE_SplashScreen_UpdateAvailableMessage"] as String, updateResult.NewVersion.ToString()), App.Current.Resources["STR_CODE_SplashScreen_UpdateAvailableHeader"] as String, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        //ToDo: use internal update mechanism and do not rely on browser
                        var downRes = Code.UpdateManager.Instance.DownloadUpdate(updateResult, new Progress<Tuple<double, long>>((val) =>
                        {
                            worker.ReportProgress((int)((val.Item1 / val.Item2) * 100), string.Format("Downloading update ({0}kb/{1}kb)", (long)val.Item1 / 1024, val.Item2 / 1024));
                        }));
                        while (!downRes.IsCompleted)
                        {
                            Thread.Sleep(100);
                        }
                        System.Diagnostics.Process.Start(downRes.Result);
                        doShutdown = true;
                    }
                }
                else
                {
                    worker.ReportProgress(100, "No Update available");
                }

            }
#endif
            Thread.Sleep(1000);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(doShutdown)
            {
                App.Current.Shutdown();
            }
            else if (isFinished)
            {
                App.Current.MainWindow = new MainWindow();
                App.Current.MainWindow.Show();
            }
        }
    }
}
