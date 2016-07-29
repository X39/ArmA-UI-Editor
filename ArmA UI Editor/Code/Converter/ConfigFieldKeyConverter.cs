using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using SQF.ClassParser;

namespace ArmA_UI_Editor.Code.Converter
{
    public abstract class ConfigFieldKeyConverterBase : IValueConverter
    {
        public string Key { get; protected set; }

        public ConfigFieldKeyConverterBase(string key)
        {
            this.Key = key;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = AddInManager.Instance.MainFile.GetKey(this.Key, ConfigField.KeyMode.NullOnNotFound);
            if (field == null)
                return null;
            else
                return this.DoConvert(field, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            value = this.DoConvertBack(value, targetType, parameter, culture);
            if (value != null)
            {
                var field = AddInManager.Instance.MainFile.GetKey(this.Key, ConfigField.KeyMode.CreateNew);
                field.Parent.SetKey(field.Name, value);
            }
            return AddInManager.Instance.MainFile.Value;
        }
        public abstract object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture);
        public abstract object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
