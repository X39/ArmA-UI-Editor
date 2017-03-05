using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVirtuality.SQF
{
    public class SQFCommandBinary : SQFCommand
    {
        public SQFExpression LeftExpression { get; set; }
        public SQFExpression RightExpression { get; set; }
        public SQFDefinition CommandDefinition { get; set; }
        public SQFCommandBinary(SQFDefinition def, SQFExpression lExp, SQFExpression rExp) : base(def)
        {
            this.CommandDefinition = def;
            this.LeftExpression = lExp;
            this.RightExpression = rExp;
        }
    }
}
