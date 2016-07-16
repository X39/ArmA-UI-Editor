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
using ArmA_UI_Editor.Code.AddInUtil.PropertyUtil;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    public class Property
    {

        [XmlElement("field")]
        public string FieldPath { get; set; }
        [XmlElement("name")]
        public string DisplayName { get; set; }
        [XmlElement(ElementName = "number", Type = typeof(NumberType))]
        [XmlElement(ElementName = "string", Type = typeof(StringType))]
        [XmlElement(ElementName = "boolean", Type = typeof(BooleanType))]
        [XmlElement(ElementName = "array", Type = typeof(ArrayType))]
        [XmlElement(ElementName = "listbox", Type = typeof(ListboxType))]
        public PType PropertyType { get; set; }
    }
}
