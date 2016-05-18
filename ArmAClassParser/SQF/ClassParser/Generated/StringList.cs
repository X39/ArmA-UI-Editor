using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF.ClassParser
{
    internal class StringList : List<string>
    {
        public StringList() : base() { }
        public StringList(List<string> list) : base(list) { }
    }
}
