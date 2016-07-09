using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArmA_UI_Editor.Code.Converter
{
    public class EditingSnap_ConfigFieldBufferConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SQF.ITextBuffer buffer = value as SQF.ITextBuffer;
            var str = buffer.ToString();
            var index = str.IndexOf("{") + 1;
            return str.Substring(index, str.LastIndexOf("};") - index);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
