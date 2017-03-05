using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVirtuality.SQF
{
    public class SQFCommandNull : SQFCommand
    {
        public SQFDefinition CommandDefinition { get; set; }
        public SQFCommandNull(SQFDefinition def) : base(def)
        {
            this.CommandDefinition = def;
        }
    }
}
