using System;
using System.Globalization;
using SQF.ClassParser;
using ArmA_UI_Editor.Code.AddInUtil.PropertyUtil;
using ArmA_UI_Editor.Code.AddInUtil;

namespace ArmA_UI_Editor.Code.Converter
{
    public abstract class SqfConfigFieldKeyConverter : ConfigFieldKeyConverterBase
    {
        protected PTypeDataTag Tag;
        public SqfConfigFieldKeyConverter(string key, PTypeDataTag tag) : base(key)
        {
            this.Tag = tag;
        }
        public sealed override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!value.IsString)
                return "";
            var res = SqfProperty.GetSqfPropertySectionArg(this.Tag.PropertyObject as SqfProperty, value.String, (int)this.Tag.Extra);
            return this.DoConvertFromString(res, targetType, parameter, culture);
        }

        public sealed override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = AddInManager.Instance.MainFile.GetKey(string.Concat(this.Tag.Key, this.Tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            value = this.DoConvertBackToString(value, targetType, parameter, culture);
            if (string.IsNullOrWhiteSpace(value as string))
                return null;
            return SqfProperty.SetSqfPropertySectionArg(this.Tag.PropertyObject as SqfProperty, field == null || !field.IsString ? "" : field.String, value as string, (int)this.Tag.Extra);
        }
        public abstract object DoConvertFromString(string value, Type targetType, object parameter, CultureInfo culture);
        public abstract string DoConvertBackToString(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
