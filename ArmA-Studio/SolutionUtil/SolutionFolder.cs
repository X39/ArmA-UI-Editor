using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace ArmA.Studio.SolutionUtil
{
    [XmlRoot("folder")]
    public class SolutionFolder : SolutionFileBase
    {
        public override DataTemplate GetPropertiesTemplate()
        {
            throw new NotImplementedException();
        }
        protected override void OnMouseDoubleClick(object param) { }
    }
}
