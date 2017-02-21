using System;
using NLog;
using NLog.Targets;

namespace ArmA.Studio.LoggerTargets
{
    [Target("SubscribableTarget")]
    public sealed class SubscribableTarget : TargetWithLayout
    {
        public SubscribableTarget()
        {
            this.Name = "SubscribableTarget";
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
}