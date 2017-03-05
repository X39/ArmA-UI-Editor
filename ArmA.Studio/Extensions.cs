using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmA.Studio
{
    public static class Extensions
    {
        public static string GetIniSectionName(this object obj)
        {
            return obj.GetType().FullName;
        }
    }
}
