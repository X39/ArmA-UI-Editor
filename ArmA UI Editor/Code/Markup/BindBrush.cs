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

namespace ArmA_UI_Editor.Code.Markup
{
    public class BindBrush : BindConfig
    {
        public BindBrush() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (CurrentConfig == null)
                return Brushes.Red;
            var data = CurrentConfig.ReceiveFieldFromHirarchy(CurrentClassPath, this.Path);
            if (data == null)
                throw new Exception(string.Format("Cannot locate field '{0}'", this.Path));
            var array = data.Array;
            return new SolidColorBrush(Color.FromArgb((byte)(array[3].Number * 256), (byte)(array[0].Number * 256), (byte)(array[1].Number * 256), (byte)(array[2].Number * 256)));
        }
    }
}
