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
using SQF.ClassParser;
using System.Windows.Data;
using System.Globalization;
using ArmA_UI_Editor.Code.Converter;

namespace ArmA_UI_Editor.Code.AddInUtil.PropertyUtil
{
    public class ListboxType : PType
    {
        public class SqfPropertyConverter : Code.Converter.SqfConfigFieldKeyConverter
        {
            public SqfPropertyConverter(string key, PTypeDataTag tag, List<Data> dataList) : base(key, tag)
            {
                this.DataList = dataList;
            }

            public List<Data> DataList { get; private set; }

            public override string DoConvertBackToString(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}", this.DataList[(int)value].Value);
            }

            public override object DoConvertFromString(string value, Type targetType, object parameter, CultureInfo culture)
            {
                for (int i = 0; i < this.DataList.Count; i++)
                {
                    if (this.DataList[i].Value == value)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }
        public class NormalPropertyConverter : Code.Converter.ConfigFieldKeyConverterBase
        {
            public NormalPropertyConverter(string key, List<Data> dataList) : base(key)
            {
                this.DataList = dataList;
            }

            public List<Data> DataList { get; private set; }

            public override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
            {
                var str = string.Format(CultureInfo.InvariantCulture, "{0}", value.Value);
                for (int i = 0; i < this.DataList.Count; i++)
                {
                    if (this.DataList[i].Value == str)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var selectedData = this.DataList[(int)value];
                switch (selectedData.Type.ToUpper())
                {
                    default:
                        value = selectedData.Value;
                        break;
                    case "NUMBER":
                        value = double.Parse(selectedData.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                }
                return value;
            }
        }

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
            cb.ItemsSource = this.Items;

            var binding = new Binding("Value");
            binding.Source = AddInManager.Instance.MainFile;
            binding.NotifyOnSourceUpdated = true;
            
            if (tag.PropertyObject is SqfProperty)
            {
                binding.Converter = new SqfPropertyConverter(Key, tag, this.Items);
                binding.ConverterParameter = tag;
            }
            else
            {
                binding.Converter = new NormalPropertyConverter(Key, this.Items);
            }
            cb.SetBinding(ComboBox.SelectedIndexProperty, binding);
            cb.SourceUpdated += Cb_SourceUpdated;
            cb.ToolTip = App.Current.Resources["STR_CODE_Property_ValueList"] as String;
            return cb;
        }

        private void Cb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.RaiseValueChanged(sender, (PTypeDataTag)(sender as FrameworkElement).Tag);
        }
    }
}