using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;
using System.Windows.Controls;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    [Serializable]
    [XmlRoot("root")]
    public class Properties
    {
        public class Property
        {
            public class PTypeDataTag
            {
                public SQF.ClassParser.File File;
                public SQF.ClassParser.Data BaseData;
                public string Path;
            }
            public abstract class PType
            {
                public abstract FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal);
                public static event EventHandler ValueChanged;
                protected void TriggerValueChanged(object sender)
                {
                    if (ValueChanged != null)
                        ValueChanged(sender, new EventArgs());
                }
            }
            public class StringType : PType
            {
                public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal)
                {
                    var tb = new TextBox();
                    tb.Text = curVal != null ? curVal.String : "";
                    tb.PreviewTextInput += Tb_PreviewTextInput;
                    return tb;
                }

                private void Tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
                {
                    if (e.Text.Contains('\r'))
                    {
                        var tb = sender as TextBox;
                        PTypeDataTag tag = tb.Tag as PTypeDataTag;
                        var data = tag.File[tag.Path];
                        if (data == null)
                            data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                        data.String = tb.Text;
                        TriggerValueChanged(tb);
                    }
                }
            }
            public class NumberType : PType
            {
                public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal)
                {
                    var tb = new TextBox();
                    tb.Text = curVal != null ? curVal.Number.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
                    tb.PreviewTextInput += Tb_PreviewTextInput;
                    return tb;
                }

                private void Tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
                {
                    if(e.Text.Contains('\r'))
                    {
                        var tb = sender as TextBox;
                        PTypeDataTag tag = tb.Tag as PTypeDataTag;
                        var data = tag.File[tag.Path];
                        if (data == null)
                            data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                        data.Number = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                        TriggerValueChanged(tb);
                    }
                }
            }
            public class BooleanType : PType
            {
                public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal)
                {
                    var cb = new ComboBox();
                    cb.Items.Add("true");
                    cb.Items.Add("false");
                    if(curVal != null)
                        cb.SelectedIndex = curVal.Boolean ? 0 : 1;
                    cb.SelectionChanged += Cb_SelectionChanged;
                    return cb;
                }

                private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    ComboBox cb = sender as ComboBox;
                    PTypeDataTag tag = cb.Tag as PTypeDataTag;
                    var data = tag.File[tag.Path];
                    if (data == null)
                        data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                    data.Boolean = bool.Parse((string)cb.SelectedValue);
                    TriggerValueChanged(cb);
                }
            }
            public class ArrayType : PType
            {
                [XmlElement("type")]
                public string Type { get; set; }
                [XmlElement("count")]
                public int Count { get; set; }

                public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal)
                {
                    var tb = new TextBlock();
                    tb.Text = "Not Yet Implemented :(";
                    return tb;
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

                public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal)
                {
                    var cb = new ComboBox();
                    cb.DisplayMemberPath = "Name";
                    cb.SelectedValuePath = "Value";
                    foreach (var it in this.Items)
                    {
                        cb.Items.Add(it);
                        if (curVal != null && it.Value == curVal.String)
                            cb.SelectedItem = it;
                    }
                    cb.SelectionChanged += Cb_SelectionChanged;
                    return cb;
                }

                private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    ComboBox cb = sender as ComboBox;
                    PTypeDataTag tag = cb.Tag as PTypeDataTag;
                    var data = tag.File[tag.Path];
                    if (data == null)
                        data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                    data.String = (string)cb.SelectedValue;
                    TriggerValueChanged(cb);
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
