using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ArmA.Studio.UI;

namespace ArmA.Studio.SolutionUtil
{
    [XmlRoot("file")]
    public sealed class SolutionFile : UI.ViewModel.IPropertiesViewModel
    {
        [XmlAttribute]
        public string RelativePath { get { return this._RelativePath; } set { this._RelativePath = value; this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RelativePath")); } }
        private string _RelativePath;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
