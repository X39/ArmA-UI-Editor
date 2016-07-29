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
    public class BindImage : BindConfig
    {
        public BindImage() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var bmp = new System.Windows.Media.Imaging.BitmapImage(new Uri(BindConfig.AddInPath + this.Path));
            
            return bmp;
        }
    }
}
