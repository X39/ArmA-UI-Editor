using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xaml;
using System.Windows;
using System.Windows.Controls;

namespace ArmA_UI_Editor.Code.Markup
{
    public class BindValue : BindConfig
    {
        public BindValue() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(CurrentClassPath))
                return "NA";
            var field = AddInManager.Instance.MainFile.GetKey(string.Concat(CurrentClassPath, this.Path), SQF.ClassParser.ConfigField.KeyMode.CheckParentsNull);
            if (field == null)
                throw new Exception(string.Format(App.Current.Resources["STR_BINDING_UnknownField"] as string, string.Concat(CurrentClassPath, this.Path)));
            return field.Value;
        }
    }
}
