using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmA.Studio
{
    public struct SyntaxErrorMarker
    {
        public int Offset { get; set; }
        public int Length { get; set; }
    }
}
