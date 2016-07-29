using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF.ClassParser.Generated
{
    public class RangeDescription
    {
        public int WholeStart { get; internal set; }
        public int WholeEnd { get; internal set; }
        public int NameStart { get; internal set; }
        public int NameEnd { get; internal set; }
        public int ValueStart { get; internal set; }
        public int ValueEnd { get; internal set; }
        public bool IsFilled { get { return this.WholeStart != -1 && this.WholeEnd != -1 && this.NameStart != -1 && this.NameEnd != -1 && this.ValueStart != -1 && this.ValueEnd != -1; } }
        public RangeDescription()
        {
            this.WholeStart = -1;
            this.WholeEnd = -1;
            this.NameStart = -1;
            this.NameEnd = -1;
            this.ValueStart = -1;
            this.ValueEnd = -1;
        }
    }
}
