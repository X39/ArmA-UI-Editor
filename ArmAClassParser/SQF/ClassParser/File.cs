using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * This is pretty much bullshit dump code ...
 * Needs to be rewritten as soon as i start caring about it
 */

namespace SQF.ClassParser
{
    public class File
    {
        private File()
        {
            
        }
        public static File load(string filePath)
        {
            System.IO.FileStream file = System.IO.File.Open(filePath, System.IO.FileMode.Open);
            var res = load(file);
            file.Close();
            return res;
        }
        public static File load(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
