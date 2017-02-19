using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Xml;

namespace ArmA.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum ExitCodes
        {
            NoWorkspaceSelected = -1,
            OK = 0,
            Restart = 1
        }
        public static string ExecutableDirectory { get { return Path.GetDirectoryName(ExecutableFile); } }
        public static string ExecutableFile { get { return Assembly.GetExecutingAssembly().GetName().CodeBase.Substring("file:///".Length); } }
        public static string SyntaxFilesPath { get { return Path.Combine(ExecutableDirectory, "SyntaxFiles"); } }
        public static string ConfigPath { get { return Path.Combine(ExecutableDirectory, "Configuration"); } }
        public static string TempPath { get { return Path.Combine(Path.GetTempPath(), @"X39\ArmA-Studio"); } }
        public static string CommonApplicationDataPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"X39\ArmA-Studio"); } }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //ToDo: Add NLog Logger
            /*
            var target = new UI.Snaps.OutputSnap.EventedTarget(); //NLog.Targets.TargetWithLayout
            NLog.Config.ConfigurationItemFactory.Default.Targets.RegisterDefinition("EventedTarget", typeof(UI.Snaps.OutputSnap.EventedTarget));
            LogManager.Configuration.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Info, target));
            LogManager.Configuration.AddTarget(target);
            LogManager.ReconfigExistingLoggers();
            */

            if (ConfigHost.Instance.AppIni["App"] == null)
            {
                ConfigHost.Instance.AppIni.Sections.AddSection("App");
            }

            var workspace = ConfigHost.Instance.AppIni["App"]["workspace"];
            if (string.IsNullOrWhiteSpace(workspace) && !SwitchWorkspace())
            {
                this.Shutdown((int)ExitCodes.NoWorkspaceSelected);
                return;
            }
            workspace = ConfigHost.Instance.AppIni["App"]["workspace"];
            Workspace.CurrentWorkspace = new Workspace(workspace);
            var mwnd = new MainWindow();
            mwnd.Show();
        }

        public static bool SwitchWorkspace()
        {
            var dlgDc = new Dialogs.WorkspaceSelectorDialogDataContext();
            var dlg = new Dialogs.WorkspaceSelectorDialog(dlgDc);
            var dlgResult = dlg.ShowDialog();
            if (!dlgResult.HasValue || !dlgResult.Value)
            {
                return false;
            }
            var workspace = dlgDc.CurrentPath;
            ConfigHost.Instance.AppIni["App"]["workspace"] = workspace;
            return true;
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG
            Debugger.Break();
#endif
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            
            Workspace.CurrentWorkspace = null;
            ConfigHost.Instance.Save();
            if(e.ApplicationExitCode == (int)ExitCodes.Restart)
            {
                Process.Start(ExecutableFile);
            }
        }

        public static void Shutdown(ExitCodes code)
        {
            App.Current.Shutdown((int)code);
        }
    }
}
