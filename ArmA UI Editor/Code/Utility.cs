using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmA_UI_Editor.Code
{
    public static class Utility
    {
        public static System.IO.Stream ToStream(this string str)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return stream;
        }
    }
}
