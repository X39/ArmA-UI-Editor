using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF.ClassParser
{
    public class KeyNotFoundException : System.Collections.Generic.KeyNotFoundException
    {
        public KeyNotFoundException(string msg, string key) : base(msg)
        {
            this.Key = key;
        }
        public KeyNotFoundException(string msg, string key, Exception inner) : base(msg, inner)
        {
            this.Key = key;
        }

        public string Key { get; private set; }
    }
}
