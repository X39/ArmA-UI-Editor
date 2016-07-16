using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using ArmA_UI_Editor.UI.Snaps;

namespace ArmA_UI_Editor.Code.AddInUtil.PropertyUtil
{
    public struct PTypeDataTag
    {
        public string Key;
        public string Path;
        public object PropertyObject;
        public object Extra;
    }
    public abstract class PType
    {
        public abstract FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag);
        public static event EventHandler ValueChanged;
        public static event EventHandler<string> OnError;
        protected void TriggerValueChanged(object sender)
        {
            if (ValueChanged != null)
                ValueChanged(sender, new EventArgs());
        }
        protected void TriggerError(object sender, string msg)
        {
            if (OnError != null)
                OnError(sender, msg);
        }
    }
}
