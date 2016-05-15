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
    public class Properties : List<Properties.Group>
    {
        public class Property
        {
            public interface Type
            {
                UIElement GenerateUiElement();
            }
            public class StringType : Type
            {
                public UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class NumberType : Type
            {
                public UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class BooleanType : Type
            {
                public UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class ArrayType : Type
            {
                [XmlElement("type")]
                public string Type { get; set; }
                [XmlElement("count")]
                public int Count { get; set; }

                public UIElement GenerateUiElement()
                {
                    throw new NotImplementedException();
                }
            }
            public class ListboxType : List<ListboxType.Data>, Type
            {
                public class Data
                {
                    [XmlAttribute("display")]
                    public string Name { get; set; }
                    [XmlAttribute("value")]
                    public string Value { get; set; }
                }

                public UIElement GenerateUiElement()
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
            [XmlArray(ElementName = "listbox")]
            public Type PropertyType { get; set; }
        }
        public class Group : List<Property>
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }
    }
}
