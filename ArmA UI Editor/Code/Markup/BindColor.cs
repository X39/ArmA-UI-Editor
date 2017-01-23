using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NLog;

namespace ArmA_UI_Editor.Code.Markup
{
    public class BindColor : BindConfig
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public BindColor() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(CurrentClassPath))
                return Color.FromRgb(255, 0, 0);
            var field = AddInManager.Instance.MainFile.GetKey(string.Concat(CurrentClassPath, this.Path), SQF.ClassParser.ConfigField.KeyMode.CheckParentsNull);
            if (field == null)
                throw new Exception(string.Format(App.Current.Resources["STR_BINDING_UnknownField"] as string, string.Concat(CurrentClassPath, this.Path)));
            if (!field.IsArray)
                throw new Exception(string.Format(App.Current.Resources["STR_BINDING_InvalidFieldType"] as string, string.Concat(CurrentClassPath, this.Path), "ARRAY"));
            var array = field.Array;
            if (array.Length != 4)
                throw new Exception(string.Format(App.Current.Resources["STR_BINDING_InvalidArray_Size"] as string, string.Concat(CurrentClassPath, this.Path), 4));
            for (int i = 0; i < 4; i++)
            {
                if (!(array[i] is double))
                {
                    if (array[i] is string && (array[i] as string).Contains("profileNamespace"))
                    {
                        Logger.Warn("Detected profilenamespace for color, profilenamespace for colors is not supported. Defaulting to PINK");
                        return Colors.Pink;
                    }
                    throw new Exception(string.Format(App.Current.Resources["STR_BINDING_InvalidArray_ChildType"] as string, string.Concat(CurrentClassPath, this.Path), i, "SCALAR"));
                }
            }
            return Color.FromArgb((byte)((double)array[3] * 255), (byte)((double)array[0] * 255), (byte)((double)array[1] * 255), (byte)((double)array[2] * 255));
        }
    }
}
