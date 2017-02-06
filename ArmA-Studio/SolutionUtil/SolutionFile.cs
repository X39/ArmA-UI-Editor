using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace ArmA.Studio.SolutionUtil
{
    [XmlRoot("config")]
    public class SolutionFile : SolutionFileBase
    {
        public override ObservableCollection<SolutionFileBase> Children { get { return null; } set { } }
        public override DataTemplate GetPropertiesTemplate()
        {
            throw new NotImplementedException();
        }
    }
}
