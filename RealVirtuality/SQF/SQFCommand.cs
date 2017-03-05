using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealVirtuality.SQF
{
    public abstract class SQFCommand
    {
        public SQFDefinition Definition { get; private set; }

        public SQFCommand(SQFDefinition definition)
        {
            this.Definition = definition;
        }
    }
}
