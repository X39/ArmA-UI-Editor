using System;
using System.Globalization;
using SQF.ClassParser;
using ArmA_UI_Editor.Code.AddInUtil.PropertyUtil;
using ArmA_UI_Editor.Code.AddInUtil;

namespace ArmA_UI_Editor.Code.Converter
{
    public abstract class SqfConfigFieldKeyConverter : ConfigFieldKeyConverterBase
    {
        public SqfConfigFieldKeyConverter(string key) : base(key) { }
        public sealed override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
        {
            var tag = (PTypeDataTag)parameter;
            if (!value.IsString)
                return "";
            var res = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, value.String, (int)tag.Extra);
            return this.DoConvertBackFromString(res, targetType, culture);
        }

        public sealed override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tag = (PTypeDataTag)parameter;
            var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            return SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, field == null || !field.IsString ? "" : field.String, this.DoConvertBackFromString(value, targetType, culture), (int)tag.Extra);
        }
        public abstract object DoConvertFromString(string value, Type targetType, CultureInfo culture);
        public abstract string DoConvertBackFromString(object value, Type targetType, CultureInfo culture);
    }
}
