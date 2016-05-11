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
    public class Field
    {
        public class Value
        {
            public VType Type { get; private set; }
            public object Val { get; private set; }

            public Value(VType type, object value)
            {
                this.Type = type;
                this.Val = value;
            }
            public enum VType
            {
                Scalar,
                Array,
                String,
                Boolean
            }
            public static Value parse(System.IO.Stream stream)
            {
                int i;
                while ((i = stream.ReadByte()) >= 0)
                {
                    char c = (char)i;
                    switch (c)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            {
                                return parseScalar(stream, c);
                            }
                        case '[':
                            {
                                return parseArray(stream, c);
                            }
                        case '"':
                            {
                                return parseString(stream, c);
                            }
                        case 't':
                        case 'f':
                            {
                                return parseBoolean(stream, c);
                            }
                        default:
                            throw new Exception();
                    }
                }
                throw new Exception();
            }
            private static Value parseScalar(System.IO.Stream stream, char c)
            {
                int i = c;
                StringBuilder builder = new StringBuilder();
                do
                {
                    c = (char)i;
                    if(c == ';')
                    {
                        return new Value(VType.Scalar, int.Parse(builder.ToString()));
                    }
                    else
                    {
                        if(c == '.' || char.IsNumber(c))
                        {
                            builder.Append(c);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                while ((i = stream.ReadByte()) >= 0);
                throw new Exception();
            }
            private static Value parseArray(System.IO.Stream stream, char c)
            {
                throw new NotImplementedException();
                //Value val = null;
                //int i = c;
                //do
                //{
                //    c = (char)i;
                //    if (c == ';')
                //    {
                //
                //    }
                //    else
                //    {
                //
                //    }
                //}
                //while ((i = stream.ReadByte()) >= 0);
                //throw new Exception();
            }
            private static Value parseString(System.IO.Stream stream, char c)
            {
                Value val = null;
                int i = c;
                StringBuilder builder = new StringBuilder();
                bool escape = false;
                bool stringmode = false;
                do
                {
                    c = (char)i;
                    if (c == ';')
                    {
                        if (val == null)
                            throw new Exception();
                        else
                            return val;
                    }
                    else
                    {
                        if(c == '"' && !stringmode)
                        {
                            stringmode = true;
                        }
                        else if(c == '"' && !escape && stringmode)
                        {
                            val = new Value(VType.String, builder.ToString());
                            stringmode = false;
                        }
                        else if(stringmode)
                        {
                            if(escape)
                            {
                                escape = false;
                                switch(c)
                                {
                                    default:
                                        builder.Append(c);
                                        break;
                                    case 'n':
                                        builder.Append('\n');
                                        break;
                                    case 'r':
                                        builder.Append('\r');
                                        break;
                                    case 't':
                                        builder.Append('\t');
                                        break;
                                }
                            }
                            else
                            {
                                if(c == '\\')
                                {
                                    escape = true;
                                }
                                else
                                {
                                    builder.Append(c);
                                }
                            }
                        }
                    }
                }
                while ((i = stream.ReadByte()) >= 0);
                throw new Exception();
            }
            private static Value parseBoolean(System.IO.Stream stream, char c)
            {
                Value val = null;
                int i = c;
                do
                {
                    c = (char)i;
                    if (c == ';')
                    {
                        if (val == null)
                            throw new Exception();
                        return val;
                    }
                    else
                    {
                        if(c == 't')
                        {
                            stream.ReadByte(); //r
                            stream.ReadByte(); //u
                            stream.ReadByte(); //e
                            val = new Value(VType.Boolean, true);
                        }
                        else if(c == 'f')
                        {
                            stream.ReadByte(); //a
                            stream.ReadByte(); //l
                            stream.ReadByte(); //s
                            stream.ReadByte(); //e
                            val = new Value(VType.Boolean, false);
                        }
                    }
                }
                while ((i = stream.ReadByte()) >= 0);
                throw new Exception();
            }
        }
        public Field(string name, Value content)
        {
            this.Name = name;
            this.Content = content;
        }

        public Value Content { get; private set; }
        public string Name { get; private set; }
        static Field Parse(System.IO.Stream stream)
        {
            int i;
            bool flag_name = false;
            bool flag_value = false;
            StringBuilder name = new StringBuilder();
            Value content = null;

            while ((i = stream.ReadByte()) >= 0)
            {
                char c = (char)i;
                if(!flag_name)
                {
                    if(c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_')
                    {
                        name.Append(c);
                    }
                    else if(c == '=')
                    {
                        flag_name = true;
                    }
                    else if(c == ' ' || c == '\r' || c == '\n' || c == '\t')
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if(!flag_value)
                {
                    if (c == '=')
                    {
                        flag_value = true;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else if(flag_value)
                {
                    stream.Seek(-1, System.IO.SeekOrigin.Current);
                    content = Value.parse(stream);
                    return new Field(name.ToString(), content);
                }
                else
                {
                    throw new Exception();
                }
            }
            throw new Exception();
        }
    }
}
