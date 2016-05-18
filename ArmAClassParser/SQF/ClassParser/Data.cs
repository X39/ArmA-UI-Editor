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
        public object Value { get { return _value; } private set { if (OnValueChange != null) OnValueChange(this, new ValueChangedEventArgs(_value, value)); _value = value; } }
        public Data(ConfigClass val, string Name = "")    { this.Value = val; this.Name = Name; }
        public Data(double val, string Name = "")         { this.Value = val; this.Name = Name; }
        public Data(string val, string Name = "")         { this.Value = val; this.Name = Name; }
        public Data(bool val, string Name = "")           { this.Value = val; this.Name = Name; }
        public Data(List<double> val, string Name = "")   { this.Value = val; this.Name = Name; }
        public Data(List<string> val, string Name = "")   { this.Value = val; this.Name = Name; }
        public Data(List<bool> val, string Name = "")     { this.Value = val; this.Name = Name; }

        public bool IsClass { get { return this.Value is ConfigClass; } }
        public ConfigClass Class { get { return this.Value as ConfigClass; } set { this.Value = value; } }

        public bool IsNumber { get { return this.Value is double; } }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } set { this.Value = value; } }
        public bool IsString { get { return this.Value is string; } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } set { this.Value = value; } }
        public bool IsBoolean { get { return this.Value is bool; } }
        public bool Boolean { get { return this.Value is bool ? (bool)this.Value : default(bool); } set { this.Value = value; } }

        public bool IsArray_Number { get { return this.Value is List<double>; } }
        public List<double> Array_Number { get { return this.Value as List<double>; } set { this.Value = value; } }
        public bool IsArray_String { get { return this.Value is List<string>; } }
        public List<string> Array_String { get { return this.Value as List<string>; } set { this.Value = value; } }
        public bool IsArray_Boolean { get { return this.Value is List<bool>; } }
        public List<bool> Array_Boolean { get { return this.Value as List<bool>; } set { this.Value = value; } }

        public void WriteOut(StreamWriter writer, int tabCount = 0)
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
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write(" = ");
                writer.Write(this.Boolean.ToString());
                writer.Write(';');
            }
            else if(this.IsNumber)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write(" = ");
                writer.Write(this.Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                writer.Write(';');
            }
            else if(this.IsString)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write(" = \"");
                writer.Write(this.String);
                writer.Write("\";");
            }
            else if(this.IsArray_Boolean)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write("[] = {");
                for(int i = 0; i < this.Array_Boolean.Count; i++)
                {
                    var it = this.Array_Boolean[i];
                    if (i > 0)
                        writer.Write(", ");
                    writer.Write(it.ToString());
                }
                writer.Write("};");
            }
            else if(this.IsArray_Number)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write("[] = {");
                for (int i = 0; i < this.Array_Number.Count; i++)
                {
                    var it = this.Array_Number[i];
                    if (i > 0)
                        writer.Write(", ");
                    writer.Write(it.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                writer.Write("};");
            }
            else if(this.IsArray_String)
            {
                writer.Write(new string('\t', tabCount));
                writer.Write(this.Name);
                writer.Write("[] = {");
                for (int i = 0; i < this.Array_String.Count; i++)
                {
                    var it = this.Array_String[i];
                    if (i > 0)
                        writer.Write(", ");
                    writer.Write('"');
                    writer.Write(it);
                    writer.Write('"');
                }
                writer.Write("};");
            }
            writer.Flush();
        }
    }
}
