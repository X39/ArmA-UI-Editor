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
    public class ListboxType : PType
    {
        public class Data
        {
            public Data()
            {
                this.Name = string.Empty;
                this.Value = string.Empty;
                this.Type = string.Empty;
            }
            [XmlAttribute("display")]
            public string Name { get; set; }
            [XmlAttribute("value")]
            public string Value { get; set; }
            [XmlAttribute("as")]
            public string Type { get; set; }
        }

        [XmlArray("items")]
        [XmlArrayItem("item")]
        public List<Data> Items { get; set; }

        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var cb = new ComboBox();
            cb.Tag = tag;
            cb.DisplayMemberPath = "Name";
            cb.SelectedValuePath = "Value";
            var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            foreach (var it in this.Items)
            {
                cb.Items.Add(it);
                if (curVal != null)
                {
                    if (tag.PropertyObject is SqfProperty)
                    {
                        var val = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, curVal.String, (int)tag.Extra);
                        if (it.Value == val)
                            cb.SelectedItem = it;
                    }
                    else
                    {
                        if (it.Value == string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", curVal.Value))
                            cb.SelectedItem = it;
                    }
                }


            }
            cb.SelectionChanged += Cb_SelectionChanged;
            cb.ToolTip = App.Current.Resources["STR_CODE_Property_ValueList"] as String;
            return cb;
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            PTypeDataTag tag = (PTypeDataTag)cb.Tag;

            Data d = cb.Items[cb.SelectedIndex] as Data;
            object value = null;
            switch (d.Type.ToUpper())
            {
                default:
                    value = d.Value;
                    break;
                case "NUMBER":
                    value = double.Parse(d.Value, System.Globalization.CultureInfo.InvariantCulture);
                    break;
            }

            if (tag.PropertyObject is SqfProperty)
            {
                var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
                if (field != null && field.IsString)
                    value = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, string.IsNullOrWhiteSpace(field.String) ? "" : field.String, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", value), (int)tag.Extra);
            }
            AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), value);
            //RaiseValueChanged(value);
        }
    }
}