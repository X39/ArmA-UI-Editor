using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    [Serializable]
    [XmlRoot("root")]
    public class Properties
    {
        public class Property
        {
            public abstract class PType
            {
                public abstract UIElement GenerateUiElement();
            }
            public class StringType : PType
            {
                public override UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class NumberType : PType
            {
                public override UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class BooleanType : PType
            {
                public override UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class ArrayType : PType
            {
                [XmlElement("type")]
                public string Type { get; set; }
                [XmlElement("count")]
                public int Count { get; set; }

                public override UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class ListboxType : PType
            {
                public class Data
                {
                    [XmlAttribute("display")]
                    public string Name { get; set; }
                    [XmlAttribute("value")]
                    public string Value { get; set; }
                }

                [XmlArray("items")]
                [XmlArrayItem("item")]
                public List<Data> Items { get; set; }

                public override UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }

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
        public class Group
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlArray("properties")]
            [XmlArrayItem(ElementName = "property", Type = typeof(Property))]
            public List<Property> Items { get; set; }
        }

        [XmlArray("groups")]
        [XmlArrayItem(ElementName = "group", Type = typeof(Group))]
        public List<Properties.Group> Items { get; set; }
    }
}
