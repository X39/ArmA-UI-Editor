using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVirtuality.SQF
{
    public class SQFCommandUnary : SQFCommand
    {
        public SQFExpression RightExpression { get; set; }
        public SQFDefinition CommandDefinition { get; set; }
        public SQFCommandUnary(SQFDefinition def, SQFExpression rExp) : base(def)
        {
            this.CommandDefinition = def;
            this.RightExpression = rExp;
        }
    }
}
