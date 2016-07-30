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
    public class ArrayType : PType
    {
        public class NormalPropertyConverter : ConfigFieldKeyConverterBase
        {
            public NormalPropertyConverter(string key) : base(key) { }
            public override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
            {
                if (!value.IsArray)
                    return null;
                StringBuilder builder = new StringBuilder();
                foreach(var val in value.Array)
                {
                    builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}, ", val));
                }
                return builder.ToString().TrimEnd(',', ' ');
            }

            public override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    var writer = new System.IO.StreamWriter(memStream);
                    writer.Write("class myClass{arr[]={");
                    writer.Write(value as string);
                    writer.Write("};};");
                    writer.Flush();
                    memStream.Seek(0, System.IO.SeekOrigin.Begin);
                    SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(memStream));
                    var tmpField = p.Parse().GetKey("myClass/arr", ConfigField.KeyMode.ThrowOnNotFound);
                    if (tmpField.Array.Length != (int)parameter)
                        return null;
                    return tmpField.Value;
                }
            }
        }

        [XmlElement("type")]
        public string Type { get; set; }
        [XmlElement("count")]
        public int Count { get; set; }

        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var tb = new TextBox();
            tb.Tag = tag;
            StringBuilder builder = new StringBuilder();
            var binding = new Binding("Value");
            binding.Source = AddInManager.Instance.MainFile;
            binding.NotifyOnSourceUpdated = true;

            if (tag.PropertyObject is SqfProperty)
            {
                throw new NotImplementedException();
            }
            else
            {
                binding.Converter = new NormalPropertyConverter(Key);
            }
            binding.ConverterParameter = this.Count;
            tb.SetBinding(TextBox.TextProperty, binding);
            tb.PreviewKeyDown += Tb_PreviewKeyDown;
            tb.SourceUpdated += Tb_SourceUpdated;

            switch (Type.ToUpper())
            {
                case "NUMBER":
                    tb.ToolTip = string.Format(System.Globalization.CultureInfo.InvariantCulture, App.Current.Resources["STR_CODE_Property_Array"] as String, App.Current.Resources["STR_CODE_Property_Number"] as String, this.Count);
                    break;
                case "STRING":
                    tb.ToolTip = string.Format(System.Globalization.CultureInfo.InvariantCulture, App.Current.Resources["STR_CODE_Property_Array"] as String, App.Current.Resources["STR_CODE_Property_String"] as String, this.Count);
                    break;
                case "BOOLEAN":
                    tb.ToolTip = string.Format(System.Globalization.CultureInfo.InvariantCulture, App.Current.Resources["STR_CODE_Property_Array"] as String, App.Current.Resources["STR_CODE_Property_Boolean"] as String, this.Count);
                    break;
                default:
                    throw new Exception();
            }
            return tb;
        }

        private void Tb_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.RaiseValueChanged(sender, (PTypeDataTag)(sender as FrameworkElement).Tag);
        }

        private void Tb_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                (sender as DependencyObject).ForceBindingSourceUpdate(TextBox.TextProperty);
            }
        }
    }
}