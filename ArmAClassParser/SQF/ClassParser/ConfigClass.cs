using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF.ClassParser
{
    public class ConfigClass : Dictionary<string, Data>
    {
        public ConfigClass Parent { get; set; }
        public ConfigClass(ConfigClass Parent = default(ConfigClass)) : base()
        {
            this.Parent = Parent;
        }

        public Data this[Data d]
        {
            get { if (d == null) return null;  return this[d.Name]; }
            set { if (d != null) this[d.Name] = value; }
        }
    }
}
