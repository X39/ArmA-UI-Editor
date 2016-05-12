using System;
using System.Collections.Generic;
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
        public Data(ConfigClass val)    { this.Value = val; }
        public Data(double val)         { this.Value = val; }
        public Data(string val)         { this.Value = val; }
        public Data(bool val)           { this.Value = val; }
        public Data(List<double> val)   { this.Value = val; }
        public Data(List<string> val)   { this.Value = val; }
        public Data(List<bool> val)     { this.Value = val; }

        public bool IsClass { get { return this.Value is ConfigClass; } }
        public ConfigClass Class { get { return this.Value as ConfigClass; } set { this.Value = value; } }

        public bool IsNumber { get { return this.Value is double; } }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } set { this.Value = value; } }
        public bool IsString { get { return this.Value is string; } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } set { this.Value = value; } }
        public bool IsBoolean { get { return this.Value is string; } }
        public bool Boolean { get { return this.Value is string ? (bool)this.Value : default(bool); } set { this.Value = value; } }

        public bool IsArray_Number { get { return this.Value is string; } }
        public List<double> Array_Number { get { return this.Value as List<double>; } set { this.Value = value; } }
        public bool IsArray_String { get { return this.Value is string; } }
        public List<string> Array_String { get { return this.Value as List<string>; } set { this.Value = value; } }
        public bool IsArray_Booleanr { get { return this.Value is string; } }
        public List<bool> Array_Boolean { get { return this.Value as List<bool>; } set { this.Value = value; } }
    }
}
