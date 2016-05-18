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
            if (CurrentConfig == null)
                return "NA";
            var data = CurrentConfig.ReceiveFieldFromHirarchy(CurrentClassPath, this.Path);
            if (data == null)
                throw new Exception(string.Format("Cannot locate field '{0}'", this.Path));
            return data.Value;
        }
    }
}
