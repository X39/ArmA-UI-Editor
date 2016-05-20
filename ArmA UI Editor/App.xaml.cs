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
    }
}
