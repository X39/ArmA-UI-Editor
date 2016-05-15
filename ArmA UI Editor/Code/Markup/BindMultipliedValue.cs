using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ArmA_UI_Editor.Code.Markup
{
    internal class BindMultipliedValue : BindConfig
    {
        [ConstructorArgument("mul")]
        public double MultiplyBy { get; set; }

        public BindMultipliedValue() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (CurrentConfig == null)
                return this.MultiplyBy;
            var data = CurrentConfig[CurrentClassPath + this.Path];
            return data.Number * this.MultiplyBy;
        }
    }
}
