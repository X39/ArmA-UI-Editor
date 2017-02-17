using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace ArmA.Studio.SolutionUtil
{
    [XmlRoot("config")]
    public class SolutionFile : SolutionFileBase
    {
        public override ICommand CmdContextMenu_OpenInExplorer { get { return new UI.Commands.RelayCommand((o) => System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", this.FullPath))); } }

        public override ObservableCollection<SolutionFileBase> Children { get { return null; } set { } }

        public override DataTemplate GetPropertiesTemplate()
        {
            return null;
        }
    }
}
