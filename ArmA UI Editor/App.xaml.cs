using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using NLog;

namespace ArmA_UI_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            var path = System.IO.Path.Combine(new[] { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ArmAUiEditorCrash.txt" });
            using (var writer = new System.IO.StreamWriter(path, false))
            {
                writer.WriteLine(string.Format("Tool Version: {0}", Code.UpdateManager.Instance.AppVersion.ToString()));
                builder.AppendLine(string.Format("Tool Version: {0}", Code.UpdateManager.Instance.AppVersion.ToString()));
                var ex = e.Exception;
                int tabCount = 0;
                while (ex != null)
                {
                    writer.WriteLine(ex.Message);
                    builder.AppendLine(ex.Message);
                    writer.WriteLine(ex.StackTrace.Replace("\r", new string('\t', tabCount) + '\n'));
                    builder.AppendLine(ex.StackTrace.Replace("\r", new string('\t', tabCount) + '\n'));
                    ex = ex.InnerException;
                    tabCount++;
                }
                writer.Flush();
            }
#if DEBUG
            System.Diagnostics.Process.Start(path);
#else
            if(Settings.Instance.AutoReportCrash)
            {
                ArmA_UI_Editor.UI.CrashReportWindow repWin = new UI.CrashReportWindow();
                var res = repWin.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    builder.Insert(0, "\r\n\r\n");
                    builder.Insert(0, repWin.ReportText);
                }
                using (HttpClient client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("content", builder.ToString())
                    });
                    var response = client.PostAsync("http://x39.io/api.php?action=report&project=ArmA-UI-Editor", content).Result;
                }
                MessageBox.Show("An unhandled exception was found ...\nThe crash-log got reported (can be changed at Settings -> Options)\nThe issue will be fixed ASAP :)\n\nSorry for your lost work (in case you did not saved) ...", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(string.Format("An unhandled exception was found ...\nPlease report the crash at https://github.com/X39/ArmA-UI-Editor/issues/new\nThe issue will be fixed ASAP :)\nA CrashLog got created at '{0}'\n\nSorry for your lost work (in case you did not saved) ...", path), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#endif

            App.Current.Shutdown();
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var target = new UI.Snaps.OutputSnap.EventedTarget();
            NLog.Config.ConfigurationItemFactory.Default.Targets.RegisterDefinition("EventedTarget", typeof(UI.Snaps.OutputSnap.EventedTarget));
            LogManager.Configuration.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Info, target));
            LogManager.Configuration.AddTarget(target);
            LogManager.ReconfigExistingLoggers();

            for (var i = 0; i < e.Args.Count(); i++)
            {
                string arg = e.Args[i];
                bool hasNext = i + 1 < e.Args.Count();
                if (System.IO.File.Exists(arg))
                {
                    //ToDo: Open file tab
                }
                else
                {
                    switch(arg.ToUpper())
                    {
                        default:
                            Logger.Warn(string.Format("Unknown Startup parameter '{0}'", arg));
                            i++;
                            break;
                    }
                }
            }
        }
    }
}
