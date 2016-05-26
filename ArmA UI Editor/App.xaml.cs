using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArmA_UI_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debugger.Break();
#endif
            using (var writer = new System.IO.StreamWriter("crash.txt", false))
            {
                var ex = e.Exception;
                int tabCount = 0;
                while (ex != null)
                {
                    writer.WriteLine(ex.Message);
                    writer.WriteLine(ex.StackTrace.Replace("\r", new string('\t', tabCount) + '\n'));
                    ex = ex.InnerException;
                    tabCount++;
                }
                writer.Flush();
            }
            MessageBox.Show("An unhandled exception was found ...\nPlease report the crash.txt at https://github.com/X39/ArmA-UI-Editor/issues/new\nThe issue will be fixed ASAP :)\n\nSorry for your lost work (in case you did not saved) ...", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            App.Current.Shutdown();
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
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
                        case "LOGLEVEL":
                            if (!hasNext)
                                break;
                            switch(e.Args[i + 1].ToUpper())
                            {
                                case "0":
                                case "DEBUG":
                                    Logger.Instance.LoggingLevel = Logger.LogLevel.DEBUG;
                                    break;
                                case "1":
                                case "VERBOSE":
                                case "V":
                                    Logger.Instance.LoggingLevel = Logger.LogLevel.VERBOSE;
                                    break;

                            }
                            break;
                        case "LOGFILE":
                            if (!hasNext)
                                break;
                            Logger.Instance.setLogFile(e.Args[i + 1]);
                            break;
                        default:
                            Logger.Instance.log(Logger.LogLevel.WARNING, "Unknown Startup parameter '" + arg + "'");
                            break;
                    }
                }
            }
        }
    }
}
