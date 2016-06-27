using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF.ClassParser
{
    public class Data
    {
        public class ValueChangedEventArgs : EventArgs
        {
            public ValueChangedEventArgs(object oldValue, object newValue)
            {
                this.Old = oldValue;
                this.New = newValue;
            }

            public object New { get; private set; }
            public object Old { get; private set; }
        }
        public event EventHandler<ValueChangedEventArgs> OnValueChange;

        public string Name { get; set; }
        private object _value;
        public object Value { get { return _value; } set { if (OnValueChange != null) OnValueChange(this, new ValueChangedEventArgs(_value, value)); _value = value; } }
        public Data(ConfigClass val, string Name = "")    { this.Value = val; this.Name = Name; }
        public Data(double val, string Name = "")         { this.Value = val; this.Name = Name; }
        public Data(string val, string Name = "")         { this.Value = val; this.Name = Name; }
        public Data(bool val, string Name = "")           { this.Value = val; this.Name = Name; }
        public Data(List<Data> val, string Name = "")     { this.Value = val; this.Name = Name; }

        public bool IsClass { get { return this.Value is ConfigClass; } }
        public ConfigClass Class { get { return this.Value as ConfigClass; } set { this.Value = value; } }

        public bool IsNumber { get { return this.Value is double; } }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } set { this.Value = value; } }
        public bool IsString { get { return this.Value is string; } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } set { this.Value = value; } }
        public bool IsBoolean { get { return this.Value is bool; } }
        public bool Boolean { get { return this.Value is bool ? (bool)this.Value : default(bool); } set { this.Value = value; } }

        public bool IsArray { get { return this.Value is List<Data>; } }
        public List<Data> Array { get { return this.Value as List<Data>; } set { this.Value = value; } }

        public void WriteOut(StreamWriter writer, int tabCount = 0, bool onlyValue = false)
        {
            if(this.IsClass)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write("class ");
                writer.Write(this.Name);
                if (this.Class.Parent != null)
                {
                    writer.Write(" : ");
                    writer.Write(this.Class.Parent.Name);
                }
                writer.WriteLine();
                writer.Write(new string('\t', tabCount));
                writer.WriteLine('{');
                foreach(var it in this.Class)
                {
                    it.Value.WriteOut(writer, tabCount + 1);
                    writer.WriteLine();
                }
                writer.Write(new string('\t', tabCount));
                writer.Write("};");
            }
            else if(this.IsBoolean)
            {
                if(!onlyValue)
                {
                    writer.Write(new string('\t', tabCount));
                    writer.Write(this.Name);
                    writer.Write(" = ");
                    writer.Write(this.Boolean.ToString());
                    writer.Write(';');
                }
                else
                {
                    writer.Write(this.Boolean.ToString());
                }
            }
            else if(this.IsNumber)
            {
                if (!onlyValue)
                {
                    writer.Write(new string('\t', tabCount));
                    writer.Write(this.Name);
                    writer.Write(" = ");
                    writer.Write(this.Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    writer.Write(';');
                }
                else
                {
                    writer.Write(this.Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            else if(this.IsString)
            {
                if (!onlyValue)
                {
                    writer.Write(new string('\t', tabCount));
                    writer.Write(this.Name);
                    writer.Write(" = ");
                    writer.Write(ToSqfString(this.String));
                    writer.Write(';');
                }
                else
                {
                    writer.Write('"');
                    writer.Write(this.String);
                    writer.Write('"');
                }
            }
            else if(this.IsArray)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write("[] = {");
                for(int i = 0; i < this.Array.Count; i++)
                {
                    var it = this.Array[i];
                    if (i > 0)
                        writer.Write(", ");
                    it.WriteOut(writer, tabCount, true);
                }
                writer.Write("};");
            }
            writer.Flush();
        }

        public static string FromSqfString(string s)
        {

            s = s.Substring(1, s.Length - 2);
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"')
                {
                    if (s[i + 1] == '"')
                        i++;
                    builder.Append(c);
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
        public static string ToSqfString(string s, bool outerStringSpecifier = true)
        {
            StringBuilder builder = new StringBuilder(s.Length + 2);
            if(outerStringSpecifier)
                builder.Append('"');
            foreach (var c in s)
            {
                if(c == '"')
                {
                    builder.Append("\"\"");
                }
                else
                {
                    builder.Append(c);
                }
            }
            if (outerStringSpecifier)
                builder.Append('"');
            return builder.ToString();
        }
    }
}
