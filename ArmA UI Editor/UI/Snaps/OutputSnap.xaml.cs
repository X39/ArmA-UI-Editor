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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace ArmA_UI_Editor.UI.Snaps
{
    //ToDo: Implement NLog hook
    public partial class OutputSnap : Page, Code.Interface.ISnapWindow
    {
        [Target("EventedTarget")]
        public sealed class EventedTarget : TargetWithLayout
        {
            public EventedTarget()
            {
                this.Name = "OutputSnapTarget";
                this.Layout = "${logger}|${pad:padding=5:inner=${level:uppercase=true}} ${message}";
            }
            public event EventHandler<string> OnLog;
            protected override void Write(LogEventInfo logEvent)
            {
                if (this.OnLog == null)
                    return;
                if (logEvent.Level < LogLevel.Info)
                    return;
                this.OnLog(this, this.Layout.Render(logEvent));
            }
        }
        public OutputSnap()
        {
            InitializeComponent();
        }
        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Bottom; } }

        public void LoadSnap()
        {
            //Logger.Instance.OnLog += Logger_OnLog;
            var target = LogManager.Configuration.FindTargetByName("OutputSnapTarget") as EventedTarget;
            target.OnLog += Target_OnLog;
        }
        public void UnloadSnap()
        {
            var target = LogManager.Configuration.FindTargetByName("OutputSnapTarget") as EventedTarget;
            target.OnLog -= Target_OnLog;
        }
        private void Target_OnLog(object sender, string e)
        {
            this.LogBox.AppendText(string.Concat(e, '\n'));
        }
    }
}
