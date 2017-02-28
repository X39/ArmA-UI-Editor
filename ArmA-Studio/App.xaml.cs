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
using NLog;
using ArmA.Studio.LoggerTargets;
using NLog.Config;

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

        public static SubscribableTarget SubscribableLoggerTarget { get; private set; }
        private void SetupNLog()
        {
            //this.TraceListenerInstance = new TraceListener();
            //System.Diagnostics.Trace.Listeners.Add(this.TraceListenerInstance);
            SubscribableLoggerTarget = new SubscribableTarget();
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("SubscribableTarget", typeof(SubscribableTarget));
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, SubscribableLoggerTarget));
            LogManager.Configuration.AddTarget(SubscribableLoggerTarget);
            LogManager.ReconfigExistingLoggers();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.SetupNLog();
            var workspace = ConfigHost.App.WorkspacePath;
            if (string.IsNullOrWhiteSpace(workspace) && !SwitchWorkspace())
            {
                MessageBox.Show(Studio.Properties.Localization.WorkspaceSelectorDialog_NoWorkspaceSelected, Studio.Properties.Localization.Whoops, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown((int)ExitCodes.NoWorkspaceSelected);
                return;
            }
            workspace = ConfigHost.App.WorkspacePath;
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
            ConfigHost.App.WorkspacePath = workspace;
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
            if(e.ApplicationExitCode == (int)ExitCodes.Restart)
            {
                Process.Start(ExecutableFile);
            }
            ConfigHost.Instance.ExecSave();
        }

        public static void Shutdown(ExitCodes code)
        {
            App.Current.Shutdown((int)code);
        }
    }
}
