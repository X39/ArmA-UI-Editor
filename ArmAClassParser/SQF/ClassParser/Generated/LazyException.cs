using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF.ClassParser.Generated
{
    internal class LazyException : Exception
    {
        public int Index;
        public int Index2;
        public LazyException(int index, int index2)
        {
            this.Index = index;
            this.Index2 = index2;
        }
    }
}
