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
    public class NumberType : PType
    {
        
        private struct ConverterPropertyData
        {
            public EditingSnap Window;
            public string Conversion;
        }
        private class SqfPropertyConverter : SqfConfigFieldKeyConverter
        {
            public SqfPropertyConverter(string key, PTypeDataTag tag) : base(key, tag) { }
            public override object DoConvertFromString(string value, Type targetType, object parameter, CultureInfo culture)
            {
                ConverterPropertyData data = (ConverterPropertyData)parameter;
                switch (data.Conversion)
                {
                    default:
                        return double.Parse(value);
                    case "SCREENX":
                        return data.Window.FromSqfString(EditingSnap.FieldTypeEnum.XField, value);
                    case "SCREENY":
                        return data.Window.FromSqfString(EditingSnap.FieldTypeEnum.YField, value);
                    case "SCREENW":
                        return data.Window.FromSqfString(EditingSnap.FieldTypeEnum.WField, value);
                    case "SCREENH":
                        return data.Window.FromSqfString(EditingSnap.FieldTypeEnum.HField, value);
                }
            }
            public override string DoConvertBackToString(object value, Type targetType, object parameter, CultureInfo culture)
            {
                ConverterPropertyData data = (ConverterPropertyData)parameter;
                if(value is string)
                {
                    value = double.Parse(value as string);
                }
                switch (data.Conversion)
                {
                    default:
                        return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", value);
                    case "SCREENX":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.XField, (double)value);
                    case "SCREENY":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.YField, (double)value);
                    case "SCREENW":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.WField, (double)value);
                    case "SCREENH":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.HField, (double)value);
                }
            }
        }
        private class NormalPropertyConverter : ConfigFieldKeyConverterBase
        {
            public NormalPropertyConverter(string key) : base(key) { }
            public override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
            {
                ConverterPropertyData data = (ConverterPropertyData)parameter;
                switch (data.Conversion)
                {
                    default:
                        return value.IsNumber ? value.Number : -1;
                    case "SCREENX":
                        return value.IsNumber ? value.Number : value.IsString && !string.IsNullOrWhiteSpace(value.String) ? data.Window.FromSqfString(EditingSnap.FieldTypeEnum.XField, value.String) : -1;
                    case "SCREENY":
                        return value.IsNumber ? value.Number : value.IsString && !string.IsNullOrWhiteSpace(value.String) ? data.Window.FromSqfString(EditingSnap.FieldTypeEnum.YField, value.String) : -1;
                    case "SCREENW":
                        return value.IsNumber ? value.Number : value.IsString && !string.IsNullOrWhiteSpace(value.String) ? data.Window.FromSqfString(EditingSnap.FieldTypeEnum.WField, value.String) : -1;
                    case "SCREENH":
                        return value.IsNumber ? value.Number : value.IsString && !string.IsNullOrWhiteSpace(value.String) ? data.Window.FromSqfString(EditingSnap.FieldTypeEnum.HField, value.String) : -1;
                }
            }

            public override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                ConverterPropertyData data = (ConverterPropertyData)parameter;
                if (value is string)
                {
                    value = double.Parse(value as string, NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                switch (data.Conversion)
                {
                    default:
                        return (double)value;
                    case "SCREENX":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.XField, (double)value);
                    case "SCREENY":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.YField, (double)value);
                    case "SCREENW":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.WField, (double)value);
                    case "SCREENH":
                        return data.Window.ToSqfString(EditingSnap.FieldTypeEnum.HField, (double)value);
                }
            }
        }
        public NumberType()
        {
            this.Conversion = string.Empty;
        }
        [XmlAttribute("conversion")]
        public string Conversion { get; set; }

        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var tb = new TextBox();
            tb.Tag = tag;
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
            binding.ConverterParameter = new ConverterPropertyData { Conversion = this.Conversion.ToUpper(), Window = window };
            tb.SetBinding(TextBox.TextProperty, binding);
            tb.SourceUpdated += Tb_SourceUpdated;
            tb.PreviewTextInput += Tb_PreviewTextInput;
            tb.ToolTip = App.Current.Resources["STR_CODE_Property_Number"] as String;
            return tb;
        }

        private void Tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                (sender as DependencyObject).ForceBindingSourceUpdate(TextBox.TextProperty);
            });
        }

        private void Tb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.RaiseValueChanged(sender, (PTypeDataTag)(sender as FrameworkElement).Tag);
        }
    }
}