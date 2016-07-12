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
            return buffer.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var field = parameter as SQF.ClassParser.ConfigField;
            SQF.ITextBuffer buffer = field.ThisBuffer;
            buffer.Replace(value as string, 0, buffer.Length);
            SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(new SQF.TextBufferStream(buffer as SQF.TextBuffer)));
            p.Patch(field);
            return buffer;
        }
    }
}
