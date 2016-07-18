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
    [Serializable]
    [XmlRoot("group")]
    public class Group
    {
        [XmlRoot("root")]
        public class GroupList
        {
            [XmlElement("group")]
            public List<Group> List;

            private GroupList()
            {
                this.List = new List<Group>();
            }
        }

        internal static List<Group> Load(string path)
        {
            var x = new XmlSerializer(typeof(GroupList));
            using (var reader = new System.IO.StreamReader(path))
            {
                var result = (GroupList)x.Deserialize(reader);
                foreach(var group in result.List)
                {
                    foreach(var sqfProp in group.SqfProperties)
                    {
                        sqfProp.Arguments.Sort((arg1, arg2) => arg1.Index.CompareTo(arg2.Index));
                        foreach(var it in sqfProp.Arguments)
                        {
                            if(it.Property is ArrayType)
                                throw new Exception(string.Format("sqf property '{0}' in '{1}' has an invalid type", sqfProp.Name, path));
                        }
                    }
                }
                return result.List;
            }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("property")]
        public List<Property> Items { get; set; }
        [XmlElement("sqf")]
        public List<SqfProperty> SqfProperties { get; set; }
    }
}
