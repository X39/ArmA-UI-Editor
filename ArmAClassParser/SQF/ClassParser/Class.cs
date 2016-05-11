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
    public class Class
    {
        public Class(string name)
        {
            this.Name = name;
        }

        public List<Field> Fields { get; private set; }
        public List<Class> SubClasses { get; private set; }
        public string Name { get; private set; }

        public void AddField(Field field)
        {
            this.Fields.Add(field);
        }
        static Class Parse(System.IO.Stream stream, List<Class> ClassList)
        {
            int i;
            StringBuilder builder = new StringBuilder();
            string className = "";
            string parentName = "";
            while ((i = stream.ReadByte()) >= 0)
            {
                char c = (char)i;
                if (c == 'c')
                {
                    c = (char)(i = stream.ReadByte()); //l
                    c = (char)(i = stream.ReadByte()); //a
                    c = (char)(i = stream.ReadByte()); //s
                    c = (char)(i = stream.ReadByte()); //s
                    c = (char)(i = stream.ReadByte()); // 
                    do
                    {
                        c = (char)(i = stream.ReadByte());
                        if (c == ' ' || c == ':')
                        {
                            break;
                        }
                        builder.Append(c);
                    } while (true);
                    className = builder.ToString();
                    builder.Clear();
                    while (c == ' ')
                        c = (char)(i = stream.ReadByte());
                    
                    if (c == ':')
                    {
                        do
                        {
                            c = (char)(i = stream.ReadByte());
                            if (c == ' ' || c == '{' || c == '\r' || c == '\n')
                            {
                                break;
                            }
                            builder.Append(c);
                        } while (true);
                        parentName = builder.ToString();
                        builder.Clear();
                    }
                    while (c == ' ' || c == '\r' || c == '\n')
                        c = (char)(i = stream.ReadByte());
                }
                else if(c == '{')
                {

                }
            }
            throw new Exception();
        }
    }
}
