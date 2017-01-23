using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Xml;
using NLog;
using System.Diagnostics;
using ArmA_UI_Editor.Code;

namespace ArmA_UI_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private class TraceListener : System.Diagnostics.TraceListener
        {
            public LimitedQueue<string> StringQueue;
            private const int MAX_CACHED_MESSAGES = 1024*4;
            public TraceListener()
            {
                this.StringQueue = new LimitedQueue<string>(MAX_CACHED_MESSAGES);
            }
            private void WriteToQueue(string msg)
            {
                var messages = msg.Split('\n');
                if(messages.Length == 1 && !msg.Contains("\n"))
                {
                    if (this.StringQueue.Count == 0)
                    {
                        this.StringQueue.Push(msg);
                    }
                    else
                    {
                        this.StringQueue[this.StringQueue.Count - 1] = string.Concat(this.StringQueue[this.StringQueue.Count - 1], msg);
                    }
                }
                else
                {
                    foreach(var it in messages)
                    {
                        if (string.IsNullOrWhiteSpace(it))
                            continue;
                        this.StringQueue.Push(it);
                    }
                }
            }
            public override void Write(string message)
            {
                WriteToQueue(message);
            }

            public override void WriteLine(string message)
            {
                WriteToQueue(string.Concat(message, '\n'));
            }
        }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        private TraceListener TraceListenerInstance { get; set; }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            
            var path = System.IO.Path.Combine(new[] { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ArmAUiEditorCrash.txt" });
            using (var writer = new XmlTextWriter(new System.IO.StreamWriter(path)))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartDocument();
                writer.WriteStartElement("root");
                #region <version>
                writer.WriteStartElement("version");
                writer.WriteString(Code.UpdateManager.Instance.AppVersion.ToString());
                writer.WriteEndElement();

                #endregion
                #region <report>
                writer.WriteStartElement("report");
#if !DEBUG
                ArmA_UI_Editor.UI.CrashReportWindow repWin = new UI.CrashReportWindow();
                var res = repWin.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    writer.WriteCData(repWin.ReportText);
                }
#endif
                writer.WriteEndElement();
                #endregion
                #region <stacktrace>
                writer.WriteStartElement("stacktrace");
                var builder = new StringBuilder();
                var ex = e.Exception;
                int tabCount = 0;
                while (ex != null)
                {
                    builder.AppendLine(ex.Message);
                    builder.AppendLine(ex.StackTrace.Replace("\r", new string('\t', tabCount) + '\n'));
                    ex = ex.InnerException;
                    tabCount++;
                }
                writer.WriteCData(builder.ToString());
                writer.WriteEndElement();
                #endregion
                #region <trace>
                writer.WriteStartElement("trace");
                foreach (var it in this.TraceListenerInstance.StringQueue)
                {
                    writer.WriteStartElement("log");
                    writer.WriteString(it.Replace("\r", ""));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                #endregion

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
#if DEBUG
            System.Diagnostics.Process.Start(path);
#else
            if (Settings.Instance.AutoReportCrash)
            {
                using (var file = System.IO.File.OpenRead(path))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("content", new System.IO.StreamReader(file).ReadToEnd())
                        });
                        var response = client.PostAsync("http://x39.io/api.php?action=report&project=ArmA-UI-Editor", content).Result;
                    }
                }
                MessageBox.Show(string.Format("An unhandled exception was found ...\nThe crash-log got reported (can be changed at Settings -> Options)\nThe issue will be fixed ASAP :)\n\nSorry for your lost work (in case you did not saved) ...\nData Transfered can be reviewed at '{0}'", path), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            this.TraceListenerInstance = new TraceListener();
            System.Diagnostics.Trace.Listeners.Add(this.TraceListenerInstance);
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
