using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQF;

namespace SQF.ClassParser
{
    public class ConfigField : INotifyPropertyChanged, INotifyPropertyChanging
    {
        internal enum MarkOffsets
        {
            NA = -1,
            front,
            name,
            name_parent,
            parent,
            parent_value,
            value,
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
            public Tuple<string, int> GetRange(ConfigField inField)
            {
                var field = inField;
                int thisOffset = 0;
                for(int i = (int)this.ArrayOffset - 1; i >= 0; i--)
                {
                    thisOffset += field.Marks[i].Length;
                }
                field = field.Parent;

                while (field != null)
                {
                    for (int i = (int)MarkOffsets.parent_value; i >= 0; i--)
                    {
                        thisOffset += field.Marks[i].Length;
                    }
                    field = field.Parent;
                }
                return new Tuple<string, int>(inField.ThisBuffer.Substring(thisOffset, this.Length), thisOffset + this.Length);
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
        private const string EX_INVALIDTYPE_CLASSARRAY = "ConfigField is neither of type Class nor of type Array";
        private const string EX_INVALIDARG_KEYALREADYEXISTS = "ConfigField already contains provided key";
        private const string EX_INVALIDARG_KEYNOTFOUND = "ConfigField does not contains provided key";
        private const string EX_INVALIDARG_KEYNOTFOUNDHIRARCHY = "provided key is not existing in hirarchy";
        private const string EX_INVALIDARG_INVALIDKEY = "Provided key contains unallowed characters. Only AlphaNumeric characters and '_' are allowed.";
        private const string EX_INVALIDARG_FIELDNOTFOUND = "ConfigField does not contains provided field";
        private const string EX_INVALIDARG_SELFREFERENCE = "Key provided equals this name";
        private const string EX_INVALIDARG_INVALIDVALUE = "Value provided is not valid for ConfigFields";
        private const string EX_INVALIDOPS_ALREADYCLASS = "ConfigField is already an Class";
        private const string EX_INVALIDOPS_ALREADYFIELD = "ConfigField is already a Field";

        #endregion

        private object _Value;
        private string _Name;
        private string _ConfigParentName;
        private TextBuffer _ThisBuffer;
        internal Mark[] Marks = new Mark[(int)MarkOffsets.blockclose + 1];

        public ConfigField Parent { get; private set; }
        public string Name { get { return _Name; } internal set { if (_Name != null && _Name.Equals(value)) return;  this.RaisePropertyChanging(); _Name = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.name); } }
        public string ConfigParentName { get { return _ConfigParentName; } internal set { if (_ConfigParentName != null && _ConfigParentName.Equals(value)) return; this.RaisePropertyChanging(); _ConfigParentName = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.parent); } }
        internal TextBuffer ThisBuffer { get { if (this._ThisBuffer == default(TextBuffer)) return this.Parent == default(ConfigField) ? default(TextBuffer) : this.Parent.ThisBuffer; return this._ThisBuffer; } }
        public object Value { get { return _Value; } }

        public bool IsArray { get { return this.Value != null && this.Value.GetType().IsArray; } }
        public bool IsClass { get { return this.Value is List<ConfigField>; } }
        public bool IsNumber { get { return this.Value is double; } }
        public bool IsString { get { return this.Value is string; } }
        public bool IsBoolean { get { return this.Value is bool; } }
        private List<ConfigField> Children { get { return this.Value as List<ConfigField>; } set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.NA); } }
        public object[] Array { get { return this.Value as object[]; } internal set { if (this._Value != null && this._Value.Equals(value)) return; this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } internal set { if (this._Value != null && this._Value.Equals(value)) return; this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } internal set { if (this._Value != null && this._Value.Equals(value)) return; this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public bool Boolean { get { return this.Value is bool ? (bool)this.Value : default(bool); } internal set { if (this._Value != null && this._Value.Equals(value)) return; this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); UpdateTextBuffer(MarkOffsets.value); } }
        public int Count { get { if (!this.IsClass) throw new ArgumentException(EX_INVALIDTYPE_CLASS); return this.Children.Count; } }
        public string Key
        {
            get
            {
                List<string> nameList = new List<string>();
                ConfigField field = this;
                while(field != null)
                {
                    nameList.Add(field.Name);
                    field = field.Parent;
                }
                return string.Join("/", nameList.ToArray());
            }
        }

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
            this.Marks[(int)MarkOffsets.name_parent] = new Mark(0, MarkOffsets.name_parent);
            this.Marks[(int)MarkOffsets.parent] = new Mark(0, MarkOffsets.parent);
            this.Marks[(int)MarkOffsets.parent_value] = new Mark(0, MarkOffsets.value);
            this.Marks[(int)MarkOffsets.value] = new Mark(0, MarkOffsets.value);
            this.Marks[(int)MarkOffsets.blockclose] = new Mark(0, MarkOffsets.blockclose);
        }
        public ConfigField(TextBuffer buffer)
        {
            this.ToClass();
            this.Parent = default(ConfigField);
            this._ThisBuffer = buffer;
            this._ConfigParentName = default(string);
            this._Name = string.Empty;
            this.Marks[(int)MarkOffsets.front] = new Mark(0, MarkOffsets.front);
            this.Marks[(int)MarkOffsets.name] = new Mark(0, MarkOffsets.name);
            this.Marks[(int)MarkOffsets.name_parent] = new Mark(0, MarkOffsets.name_parent);
            this.Marks[(int)MarkOffsets.parent] = new Mark(0, MarkOffsets.parent);
            this.Marks[(int)MarkOffsets.parent_value] = new Mark(0, MarkOffsets.value);
            this.Marks[(int)MarkOffsets.value] = new Mark(0, MarkOffsets.value);
            this.Marks[(int)MarkOffsets.blockclose] = new Mark(0, MarkOffsets.blockclose);
        }

        /// <summary>
        /// Creates a new empty <see cref="ConfigField"/> with given key into this class.
        /// Requires this <see cref="ConfigField"/> to be a class!
        /// </summary>
        /// <param name="key">Non-Existing key for the new field</param>
        /// <returns>new <see cref="ConfigField"/></returns>
        /// <exception cref="ArgumentException"/>
        public ConfigField AddKey(string key)
        {
            if (!ConfigField.IsValidKey(key))
                throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
            if (!this.IsArray && !this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASSARRAY);
            if (this.Contains(key))
                throw new ArgumentException(EX_INVALIDARG_KEYALREADYEXISTS);
            if (!this.IsClass)
                this.ToClass();
            ConfigField field = new ConfigField(key);
            field.Parent = this;
            this.RaisePropertyChanging("Children");
            this.Children.Add(field);
            this.RaisePropertyChanged("Children");
            return field;
        }
        /// <summary>
        /// Helper function to receive a key realtive to this <see cref="ConfigField"/>.
        /// </summary>
        /// <param name="key">Key to search</param>
        /// <param name="create">true if key should be created if not found, false otherwise</param>
        /// <returns>The <see cref="ConfigField"/> with corresponding key</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        internal ConfigField GetKey(string key, bool create)
        {
            if (!this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASS);
            if (key.Contains('/'))
            {
                //Key-path was provided
                var keys = key.Split('/');
                ConfigField currentField = this;
                for (var i = 0; i < keys.Length; i++)
                {
                    var it = keys[i];
                    if (string.IsNullOrWhiteSpace(it))
                        continue;
                    try
                    {
                        currentField = currentField[it];
                    }
                    catch (KeyNotFoundException ex)
                    {
                        if (create)
                        {
                            currentField = currentField.AddKey(it);
                            currentField.ToClass();
                        }
                        else if(string.IsNullOrWhiteSpace(currentField.ConfigParentName))
                        {
                            throw new KeyNotFoundException(ex.Message, string.Join("/", keys.GetRange(i)), ex);
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
                if(create)
                {
                    var field = this.AddKey(key);
                    field.ToClass();
                    return field;
                }
                else if (string.IsNullOrWhiteSpace(this.ConfigParentName))
                {
                    throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND, key);
                }
                else
                {
                    return this.FindConfigKeyInHirarchy(this.ConfigParentName)[key];
                }
            }
        }
        /// <summary>
        /// Changes the value of underlying <see cref="ConfigField"/> with given key in this class.
        /// Key will be added automatically if not yet in this <see cref="ConfigField"/>.
        /// Requires this <see cref="ConfigField"/> to be a class!
        /// </summary>
        /// <param name="key">Key for the field</param>
        /// <param name="value"><para>Value to set this to.</para>
        /// <para>Can be of following types:
        ///    <list type="bullet">
        ///        <item><see cref="string"/></item>
        ///        <item><see cref="double"/></item>
        ///        <item><see cref="bool"/></item>
        ///        <item><see cref="object"/>[] containing any combination of <see cref="string"/>, <see cref="double"/> or <see cref="bool"/></item>
        ///     </list>
        /// </para>
        /// </param>
        /// <returns>new <see cref="ConfigField"/></returns>
        /// <exception cref="ArgumentException"/>
        public void SetKey(string key, object value)
        {
            if (!ConfigField.IsValidKey(key))
                throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
            if (!this.IsArray && !this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASSARRAY);
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
                var arr = (object[])value;
                foreach(var it in arr)
                {
                    if (it is string || it is bool || it is double)
                        continue;
                    else
                        throw new ArgumentException(EX_INVALIDARG_INVALIDVALUE);
                }
                field.Array = arr;
            }
            else
            {
                throw new ArgumentException(EX_INVALIDARG_INVALIDVALUE);
            }
        }
        /// <summary>
        /// Removes <see cref="ConfigField"/> with corresponding key from this class.
        /// Requires this <see cref="ConfigField"/> to be a class!
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
        /// <para>Receives <see cref="ConfigField"/> with given key from this class.
        /// Requires this <see cref="ConfigField"/> to be a class!</para>
        /// <para>Will receive key relative to this <see cref="ConfigField"/> if key contains the separator char <value>/</value></para>
        /// <para>Function will search parent classes too</para>
        /// </summary>
        /// <param name="key">key to receive</param>
        /// <returns><see cref="ConfigField"/> with given key</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        public ConfigField this[string key] { get { return this.GetKey(key, false); } }
        /// <summary>
        /// <para>Receives <see cref="ConfigField"/> with given index from this class.
        /// Requires this <see cref="ConfigField"/> to be a class!</para>
        /// </summary>
        /// <param name="index">Index to receive</param>
        /// <returns><see cref="ConfigField"/> with given index</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ConfigField this[int index]
        {
            get
            {
                if (!this.IsClass)
                    throw new ArgumentException(EX_INVALIDTYPE_CLASS);
                if (index < 0 || this.Children.Count <= index)
                    throw new ArgumentOutOfRangeException();
                return this.Children[index];
            }
        }
        /// <summary>
        /// Changes this <see cref="ConfigField"/> to a class.
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
        /// Changes this <see cref="ConfigField"/> to a field.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void ToField()
        {
            if (!this.IsClass)
                throw new InvalidOperationException(EX_INVALIDOPS_ALREADYFIELD);
            this.Children = null;
        }
        /// <summary>
        /// Checks if given key exists in this class.
        /// Requires this <see cref="ConfigField"/> to be a class!
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
            if (this.ThisBuffer == null)
                return;
            string replaceText = string.Empty;
            switch (mo)
            {
                case MarkOffsets.front:
                    replaceText = string.Concat(new string('\t', this.ParentCount), this.IsClass ? "class " : "");
                    break;
                case MarkOffsets.name:
                    replaceText = this.Name;
                    break;
                case MarkOffsets.name_parent:
                    replaceText = this.IsClass && !string.IsNullOrWhiteSpace(this.ConfigParentName) ? " : " : "";
                    break;
                case MarkOffsets.parent:
                    replaceText = string.IsNullOrWhiteSpace(this.ConfigParentName)  ? "" : this.ConfigParentName;
                    break;
                case MarkOffsets.parent_value:
                    replaceText = this.IsClass ? string.Format("\r\n{0}{{", new string('\t', this.ParentCount)) : this.IsArray ? "[] = " : " = ";
                    break;
                case MarkOffsets.value:
                    if (this.IsClass)
                        return;
                    else
                        replaceText = this.ValueToString();
                    break;
                case MarkOffsets.blockclose:
                    replaceText = this.IsClass ? string.Format("\r\n{0}}};", new string('\t', this.ParentCount)) : ";";
                    break;
                case MarkOffsets.NA:
                    UpdateTextBuffer(MarkOffsets.front);
                    UpdateTextBuffer(MarkOffsets.name);
                    UpdateTextBuffer(MarkOffsets.name_parent);
                    UpdateTextBuffer(MarkOffsets.parent);
                    UpdateTextBuffer(MarkOffsets.parent_value);
                    UpdateTextBuffer(MarkOffsets.value);
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
                builder.Append('{');
                bool flag = false;
                foreach (var val in this.Array)
                {
                    if (flag)
                        builder.Append(", ");
                    else
                        flag = true;
                    if(val is string)
                        builder.Append((val as string).ToSqfString());
                    else
                        builder.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", val));
                }
                builder.Append('}');
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
            else if (this.IsClass)
            {
                throw new ArgumentException();
            }
            else
            {
                return string.Empty.ToSqfString();
            }
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
            throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUNDHIRARCHY, key);
        }
    }
}
