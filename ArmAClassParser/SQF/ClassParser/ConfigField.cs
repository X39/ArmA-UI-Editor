using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmAClassParser.SQF;

namespace ArmAClassParser.SQF.ClassParser
{
    public class ConfigField : INotifyPropertyChanged, INotifyPropertyChanging
    {
        internal enum MarkOffsets
        {
            NA = -1,
            front,
            name,
            name_parent_space,
            parent,
            parent_blockopen_space,
            blockopen,
            blockopen_value_space,
            value,
            value_blockclose_space,
            blockclose
        }
        internal struct Mark
        {
            public int Length;
            public MarkOffsets ArrayOffset;

            public Mark(int length, MarkOffsets arrayOffset)
            {
                this.Length = length;
                this.ArrayOffset = arrayOffset;
            }

            /// <summary>
            /// Receives the textrange represented by this mark.
            /// </summary>
            /// <returns><see cref="Tuple"/> with Item1 being the actual string of given range and Item2 the int being the next offset</returns>
            public Tuple<string, int> GetRange(ConfigField field)
            {
                int thisOffset = 0;
                for(int i = (int)this.ArrayOffset; i >= 0; i--)
                {
                    thisOffset += field.Marks[i].Length;
                }
                field = field.Parent;

                while (field != null)
                {
                    for (int i = (int)MarkOffsets.blockopen_value_space; i >= 0; i--)
                    {
                        thisOffset += field.Marks[i].Length;
                    }
                    field = field.Parent;
                }
                return new Tuple<string, int>(field.ThisBuffer.Substring(thisOffset, this.Length), thisOffset + this.Length);
            }
            /// <summary>
            /// Receives the textrange represented by this mark.
            /// </summary>
            /// <param name="lastOffset">NextOffset of last Mark operation (allows for faster processing)</param>
            /// <returns><see cref="Tuple"/> with Item1 being the actual string of given range and Item2 the int being the next offset</returns>
            public Tuple<string, int> GetRange(ConfigField field, int nextOffset)
            {
                return new Tuple<string, int>(field.ThisBuffer.Substring(nextOffset, this.Length), nextOffset + this.Length);
            }
        }
        #region Eventing
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangingEventHandler PropertyChanging;
        private void RaisePropertyChanging([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanging != null)
                this.PropertyChanging(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
        }
        #endregion
        #region private Constants
        private const string EX_INVALIDTYPE_ARRAY = "ConfigField is not of type Array";
        private const string EX_INVALIDTYPE_CLASS = "ConfigField is not of type Class";
        private const string EX_INVALIDARG_KEYALREADYEXISTS = "ConfigField already contains provided key";
        private const string EX_INVALIDARG_KEYNOTFOUND = "ConfigField does not contains provided key";
        private const string EX_INVALIDARG_KEYNOTFOUNDHIRARCHY = "provided key is not existing in hirarchy";
        private const string EX_INVALIDARG_INVALIDKEY = "Provided key contains unallowed characters. Only AlphaNumeric characters and '_' are allowed.";
        private const string EX_INVALIDARG_FIELDNOTFOUND = "ConfigField does not contains provided field";
        private const string EX_INVALIDARG_SELFREFERENCE = "Key provided equals this name";
        private const string EX_INVALIDARG_INVALIDVALUE = "Value provided is not valid for ConfigFields";
        private const string EX_INVALIDOPS_ALREADYCLASS = "ConfigField is already an Class";

        #endregion

        private object _Value;
        private string _Name;
        private string _ConfigParentName;
        private TextBuffer _ThisBuffer;
        private Mark[] Marks = new Mark[(int)MarkOffsets.blockclose];


        public ConfigField Parent { get; private set; }
        public string Name { get { return _Name; } internal set { this.RaisePropertyChanging(); _Name = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.name); } }
        public string ConfigParentName { get { return _ConfigParentName; } internal set { this.RaisePropertyChanging(); _ConfigParentName = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.parent); } }
        internal TextBuffer ThisBuffer { get { if (this._ThisBuffer == default(TextBuffer)) return this.Parent.ThisBuffer; return default(TextBuffer); } }
        public object Value { get { return _Value; } }

        public bool IsArray { get { return this.Value.GetType().IsArray; } }
        public bool IsClass { get { return this.Value is List<ConfigField>; } }
        public bool IsNumber { get { return this.Value is double; } }
        public bool IsString { get { return this.Value is string; } }
        public bool IsBoolean { get { return this.Value is bool; } }
        private List<ConfigField> Children { get { return this.Value as List<ConfigField>; } set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.NA); } }
        public object[] Array { get { return this.Value as object[]; } internal set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } internal set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } internal set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public bool Boolean { get { return this.Value is bool ? (bool)this.Value : default(bool); } internal set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }

        public int ParentCount { get { int i = 0; var cf = this; while (cf.Parent != null) { i++; cf = cf.Parent; } return i; } }

        public ConfigField(string name)
        {
            this.Parent = default(ConfigField);
            this._ThisBuffer = default(TextBuffer);
            this._Value = default(object);
            this._ConfigParentName = default(string);
            this._Name = name;
            this.Marks[(int)MarkOffsets.front] = new Mark(0, MarkOffsets.front);
            this.Marks[(int)MarkOffsets.name] = new Mark(0, MarkOffsets.name);
            this.Marks[(int)MarkOffsets.name_parent_space] = new Mark(0, MarkOffsets.name_parent_space);
            this.Marks[(int)MarkOffsets.parent] = new Mark(0, MarkOffsets.parent);
            this.Marks[(int)MarkOffsets.parent_blockopen_space] = new Mark(0, MarkOffsets.parent_blockopen_space);
            this.Marks[(int)MarkOffsets.blockopen] = new Mark(0, MarkOffsets.blockopen);
            this.Marks[(int)MarkOffsets.blockopen_value_space] = new Mark(0, MarkOffsets.blockopen_value_space);
            this.Marks[(int)MarkOffsets.value] = new Mark(0, MarkOffsets.value);
            this.Marks[(int)MarkOffsets.value_blockclose_space] = new Mark(0, MarkOffsets.value_blockclose_space);
            this.Marks[(int)MarkOffsets.blockclose] = new Mark(0, MarkOffsets.blockclose);
        }

        /// <summary>
        /// Creates a new empty <see cref="ConfigField"/> with given key into this array.
        /// Requires this <see cref="ConfigField"/> to be an array!
        /// </summary>
        /// <param name="key">Non-Existing key for the new field</param>
        /// <returns>new <see cref="ConfigField"/></returns>
        /// <exception cref="ArgumentException"/>
        public ConfigField AddKey(string key)
        {
            if (!ConfigField.IsValidKey(key))
                throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
            if (!this.IsArray)
                throw new ArgumentException(EX_INVALIDTYPE_ARRAY);
            if (this.Contains(key))
                throw new ArgumentException(EX_INVALIDARG_KEYALREADYEXISTS);
            if (!this.IsClass)
                this.ToClass();
            ConfigField field = new ConfigField(key);
            field.Parent = this;
            this.RaisePropertyChanging();
            this.Children.Add(field);
            this.RaisePropertyChanged();
            return field;
        }
        public void SetKey(string key, object value)
        {
            if (!ConfigField.IsValidKey(key))
                throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
            if (!this.IsArray)
                throw new ArgumentException(EX_INVALIDTYPE_ARRAY);
            if (!this.IsClass)
                this.ToClass();
            ConfigField field;
            if (this.Contains(key))
            {
                field = this[key];
            }
            else
            {
                field = this.AddKey(key);
            }
            if (value is string)
            {
                field.String = (string)value;
            }
            else if (value is double)
            {
                field.Number = (double)value;
            }
            else if (value is bool)
            {
                field.Boolean = (bool)value;
            }
            else if(value.GetType().IsArray)
            {
                field.Array = (object[])value;
            }
            else
            {
                throw new ArgumentException(EX_INVALIDARG_INVALIDVALUE);
            }
        }
        /// <summary>
        /// Removes <see cref="ConfigField"/> with corresponding key from this array.
        /// Requires this <see cref="ConfigField"/> to be an array!
        /// </summary>
        /// <param name="field"></param>
        /// <exception cref="ArgumentException"/>
        public void RemoveKey(string key)
        {
            if (!this.IsArray)
                throw new ArgumentException(EX_INVALIDTYPE_ARRAY);
            if (!this.Contains(key))
                throw new ArgumentException(EX_INVALIDARG_KEYNOTFOUND);
            if (!this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASS);

            foreach (var field in this.Children)
            {
                if (field.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.RaisePropertyChanging();
                    this.Children.Remove(field);
                    this.RaisePropertyChanged();
                    break;
                }
            }
        }
        /// <summary>
        /// <para>Receives <see cref="ConfigField"/> with given key from this array.
        /// Requires this <see cref="ConfigField"/> to be an array!</para>
        /// <para>Will receive key relative to this <see cref="ConfigField"/> if key contains the separator char <value>/</value></para>
        /// <para>Function will search parent classes too</para>
        /// </summary>
        /// <param name="key">key to receive</param>
        /// <returns><see cref="ConfigField"/> with given key</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        public ConfigField this[string key]
        {
            get
            {
                if (!this.IsClass)
                    throw new ArgumentException(EX_INVALIDTYPE_CLASS);
                if (key.Contains('/'))
                {
                    //Key-path was provided
                    var keys = key.Split('/');
                    ConfigField currentField = this;
                    for(var i = 0; i < keys.Length; i++)
                    {
                        var it = keys[i];
                        if (string.IsNullOrWhiteSpace(it))
                            continue;
                        try
                        {
                            currentField = currentField[it];
                        }
                        catch(KeyNotFoundException ex)
                        {
                            if(string.IsNullOrWhiteSpace(currentField.ConfigParentName))
                            {
                                throw new KeyNotFoundException(ex.Message, ex);
                            }
                            else
                            {
                                StringBuilder builder = new StringBuilder();
                                currentField = this.FindConfigKeyInHirarchy(currentField.ConfigParentName)[string.Join("/", keys.GetRange(i))];
                            }
                        }
                    }
                    return currentField;
                }
                else
                {
                    //Single key was provided
                    if (!ConfigField.IsValidKey(key))
                        throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
                    foreach (var it in this.Children)
                    {
                        if (it.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                            return it;
                    }
                    if (string.IsNullOrWhiteSpace(this.ConfigParentName))
                    {
                        throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND);
                    }
                    else
                    {
                        return this.FindConfigKeyInHirarchy(this.ConfigParentName)[key];
                    }
                }
            }
        }
        /// <summary>
        /// Changes this <see cref="ConfigField"/> to an array.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void ToClass()
        {
            if (this.IsClass)
                throw new InvalidOperationException(EX_INVALIDOPS_ALREADYCLASS);
            if (this.IsArray)
            {
                object[] objArray = this.Array;
                this.Children = new List<ConfigField>();
            }
            else
            {
                this.Children = new List<ConfigField>();
            }
        }
        /// <summary>
        /// Checks if given key exists in this array.
        /// Requires this <see cref="ConfigField"/> to be an array!
        /// </summary>
        /// <param name="key">key to search</param>
        /// <returns>true if key was found, false if not</returns>
        /// <exception cref="ArgumentException"/>
        public bool Contains(string key)
        {
            if (!this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASS);

            foreach (var it in this.Children)
            {
                if (it.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Helper function to make sure provided key is valid
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>true if it is valid, false if not</returns>
        private static bool IsValidKey(string key)
        {
            return key.All((c) => char.IsLetterOrDigit(c) || c == '_');
        }
        /// <summary>
        /// Updates corresponding offsets in TextBuffer
        /// </summary>
        /// <param name="mo">Targeted <see cref="MarkOffsets"/> which to update</param>
        /// <exception cref="NotImplementedException"/>
        private void UpdateTextBuffer(MarkOffsets mo)
        {
            string replaceText = string.Empty;
            switch (mo)
            {
                case MarkOffsets.front:
                    replaceText = new string('\t', this.ParentCount);
                    break;
                case MarkOffsets.name:
                    replaceText = this.Name;
                    break;
                case MarkOffsets.name_parent_space:
                    replaceText = this.IsClass ? " : " : "";
                    break;
                case MarkOffsets.parent:
                    replaceText = this.ConfigParentName;
                    break;
                case MarkOffsets.parent_blockopen_space:
                    replaceText = this.IsClass ? string.Format("\r\n{0}", new string('\t', this.ParentCount)) : " ";
                    break;
                case MarkOffsets.blockopen:
                    replaceText = this.IsClass ? "{" : "=";
                    break;
                case MarkOffsets.blockopen_value_space:
                    replaceText = this.IsClass ? string.Format("\r\n{0}", new string('\t', this.ParentCount)) : " ";
                    break;
                case MarkOffsets.value:
                    if (this.IsClass)
                        return;
                    else
                        replaceText = this.ValueToString();
                    break;
                case MarkOffsets.value_blockclose_space:
                    replaceText = this.IsClass ? string.Format("\r\n{0}", new string('\t', this.ParentCount)) : "";
                    break;
                case MarkOffsets.blockclose:
                    replaceText = this.IsClass ? "};" : ";";
                    break;
                case MarkOffsets.NA:
                    UpdateTextBuffer(MarkOffsets.name);
                    UpdateTextBuffer(MarkOffsets.name_parent_space);
                    UpdateTextBuffer(MarkOffsets.parent);
                    UpdateTextBuffer(MarkOffsets.parent_blockopen_space);
                    UpdateTextBuffer(MarkOffsets.blockopen);
                    UpdateTextBuffer(MarkOffsets.blockopen_value_space);
                    UpdateTextBuffer(MarkOffsets.value);
                    UpdateTextBuffer(MarkOffsets.value_blockclose_space);
                    UpdateTextBuffer(MarkOffsets.blockclose);
                    return;
            }
            this.ThisBuffer.Replace(replaceText, this.Marks[(int)mo].GetRange(this).Item2, this.Marks[(int)mo].Length);
            this.Marks[(int)mo].Length = replaceText.Length;
        }
        /// <summary>
        /// Converts value to write-to-file comform string representation
        /// </summary>
        /// <returns>Write-Out ready string representation of this value</returns>
        /// <exception cref="ArgumentException"/>
        private string ValueToString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.IsArray)
            {
                if(this.IsClass)
                {
                    throw new ArgumentException();
                }
                else
                {
                    builder.Append('{');
                    bool flag = false;
                    foreach(var val in this.Children)
                    {
                        if (flag)
                            builder.Append(", ");
                        else
                            flag = true;
                        builder.Append(val.ValueToString());
                    }
                    builder.Append('}');
                }
            }
            else if (this.IsBoolean)
            {
                var val = this.Boolean;
                builder.Append(val.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (this.IsNumber)
            {
                var val = this.Number;
                builder.Append(val.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (this.IsString)
            {
                var val = this.String;
                builder.Append(val.ToSqfString());
            }
#if DEBUG
            else
            {
                throw new Exception();
            }
#endif
            return builder.ToString();
        }
        /// <summary>
        /// Searches for provided key in parents hirarchy.
        /// </summary>
        /// <param name="key">Key to search for</param>
        /// <returns><see cref="ConfigField"/> in field hirarchy</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        public ConfigField FindConfigKeyInHirarchy(string key)
        {
            if (key.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException(EX_INVALIDARG_SELFREFERENCE);
            var curConf = this.Parent;
            while (curConf != null)
            {
                foreach(var it in curConf.Children)
                {
                    if(it.IsClass && it.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return it;
                    }
                }
                curConf = curConf.Parent;
            }
            throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUNDHIRARCHY);
        }
    }
}
