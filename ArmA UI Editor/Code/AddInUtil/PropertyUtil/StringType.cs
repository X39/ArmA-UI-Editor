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
using System.Windows.Data;
using System.Globalization;
using ArmA_UI_Editor.Code.Converter;
using SQF.ClassParser;


namespace ArmA_UI_Editor.Code.AddInUtil.PropertyUtil
{
    public class StringType : PType
    {
        private class SqfPropertyConverter : SqfConfigFieldKeyConverter
        {
            public SqfPropertyConverter(string key, PTypeDataTag tag) : base(key, tag) { }
            public override object DoConvertFromString(string value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
            public override string DoConvertBackToString(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value as string;
            }
        }
        private class NormalPropertyConverter : ConfigFieldKeyConverterBase
        {
            public NormalPropertyConverter(string key) : base(key) { }
            public override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.IsString ? value.String : string.Empty;
            }

            public override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }
        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var tb = new TextBox();
            tb.Tag = tag;
            var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.EmptyReferenceOnNotFound);
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
            tb.SetBinding(TextBox.TextProperty, binding);
            tb.SourceUpdated += Tb_SourceUpdated;
            tb.PreviewKeyDown += Tb_PreviewKeyDown;
            tb.ToolTip = App.Current.Resources["STR_CODE_Property_String"] as String;
            return tb;
        }

        private void Tb_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                (sender as DependencyObject).ForceBindingSourceUpdate(TextBox.TextProperty);
            }
        }

        private void Tb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.RaiseValueChanged(sender, (PTypeDataTag)(sender as FrameworkElement).Tag);
        }
    }
}