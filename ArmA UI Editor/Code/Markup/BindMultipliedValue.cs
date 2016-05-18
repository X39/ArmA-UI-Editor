using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ArmA_UI_Editor.Code.Markup
{
    public class BindMultipliedValue : BindConfig
    {
        [ConstructorArgument("mul")]
        public double MultiplyBy { get; set; }

        public BindMultipliedValue() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (CurrentConfig == null)
                return this.MultiplyBy;
            var data = CurrentConfig.ReceiveField(CurrentClassPath, this.Path);
            if (data == null)
                throw new Exception(string.Format("Cannot locate field '{0}'", this.Path));
            return data.Number * this.MultiplyBy;
        }
    }
}
