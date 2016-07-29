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
using System.Windows.Data;
using System.Globalization;
using ArmA_UI_Editor.Code.Converter;
using SQF.ClassParser;

namespace ArmA_UI_Editor.Code.AddInUtil.PropertyUtil
{
    public class BooleanType : PType
    {
        public class SqfPropertyConverter : SqfConfigFieldKeyConverter
        {
            public SqfPropertyConverter(string key, PTypeDataTag tag) : base(key, tag) { }
            public override object DoConvertFromString(string value, Type targetType, object parameter, CultureInfo culture)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return -1;
                return bool.Parse(value) ? 1 : 0;
            }

            public override string DoConvertBackToString(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (int)value > 0 ? "true" : "false";
            }
        }
        public class NormalPropertyConverter : ConfigFieldKeyConverterBase
        {
            public NormalPropertyConverter(string key) : base(key) { }
            public override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.IsBoolean ? value.Boolean ? 1 : 0 : -1;
            }

            public override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (int)value > 0;
            }
        }
        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var cb = new ComboBox();
            cb.Tag = tag;
            cb.Items.Add("false");
            cb.Items.Add("true");
            var binding = new Binding("Value");
            binding.Source = AddInManager.Instance.MainFile;
            binding.NotifyOnSourceUpdated = true;
            
            if (tag.PropertyObject is SqfProperty)
            {
                binding.Converter = new SqfPropertyConverter(Key, tag);
            }
            else
            {
                binding.Converter = new NormalPropertyConverter(Key);
            }
            cb.SetBinding(ComboBox.SelectedIndexProperty, binding);
            cb.SourceUpdated += Cb_SourceUpdated;
            cb.ToolTip = App.Current.Resources["STR_CODE_Property_Boolean"] as String;
            return cb;
        }

        private void Cb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.RaiseValueChanged(sender, (PTypeDataTag)(sender as FrameworkElement).Tag);
        }
    }
}