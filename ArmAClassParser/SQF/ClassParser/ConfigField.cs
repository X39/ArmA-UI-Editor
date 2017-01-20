using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQF;

namespace SQF.ClassParser
{
    public class ConfigField : INotifyPropertyChanged, INotifyPropertyChanging, IEnumerable<ConfigField>
    {
        public enum KeyMode
        {
            CreateNew,
            CheckParentsThrow,
            CheckParentsNull,
            ThrowOnNotFound,
            NullOnNotFound,
            EmptyReferenceOnNotFound,
            HighestMatchAvailable
        }
        #region Eventing
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var treeRoot = this.TreeRoot;
            if (treeRoot != this)
            {
                treeRoot.RaisePropertyChanged("Value");
            }
            else
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));

        }
        public event PropertyChangingEventHandler PropertyChanging;
        protected void RaisePropertyChanging([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {

            var treeRoot = this.TreeRoot;
            if (treeRoot != this)
            {
                treeRoot.RaisePropertyChanging("Value");
            }
            else
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

        private const string EX_INVALIDARG_INVALIDKEYMODE_SINGLEKEY = "Provided KeyMode is not valid for single-key requests";
        #endregion

        private string _Name;
        private string _ConfigParentName;
        private WeakReference<ConfigField> _Parent;
        public virtual ConfigField Parent { get { if (this._Parent == null) return null; ConfigField field; this._Parent.TryGetTarget(out field); return field; } private set { if (this._Parent == null) this._Parent = new WeakReference<ConfigField>(value); else this._Parent.SetTarget(value); } }
        public virtual string Name { get { return _Name; } set { if (_Name != null && _Name.Equals(value)) return;  this.RaisePropertyChanging(); _Name = value; this.RaisePropertyChanged(); } }
        public virtual string ConfigParentName { get { return _ConfigParentName; } set { if (_ConfigParentName != null && _ConfigParentName.Equals(value)) return; this.RaisePropertyChanging(); _ConfigParentName = value; this.RaisePropertyChanged(); } }

        public ConfigField TreeRoot
        {
            get
            {
                ConfigField field = this;
                while (field.Parent != default(ConfigField))
                {
                    field = field.Parent;
                }
                return field;
            }
        }

        private object _Value;
        public virtual object Value { get { return _Value; } set { this.RaisePropertyChanging(); this._Value = value; this.RaisePropertyChanged(); } }

        public bool IsArray { get { return this.Value != null && this.Value.GetType().IsArray; } }
        public bool IsClass { get { return this.Value is List<ConfigField>; } }
        public bool IsNumber { get { return this.Value is double; } }
        public bool IsString { get { return this.Value is string; } }
        public bool IsBoolean { get { return this.Value is bool; } }
        private List<ConfigField> Children { get { return this.Value as List<ConfigField>; } set { this.RaisePropertyChanging(); this.Value = value; this.RaisePropertyChanged(); } }
        public object[] Array { get { return this.Value as object[]; } set { if (this.Value != null && this.Value.Equals(value)) return; this.RaisePropertyChanging(); if (this.IsClass) this.ToField();  this.Value = value; this.RaisePropertyChanged();} }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } set { if (this.Value != null && this.Value.Equals(value)) return; this.RaisePropertyChanging(); if (this.IsClass) this.ToField(); this.Value = value; this.RaisePropertyChanged(); } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } set { if (this.Value != null && this.Value.Equals(value)) return; this.RaisePropertyChanging(); if (this.IsClass) this.ToField(); this.Value = value; this.RaisePropertyChanged(); } }
        public bool Boolean { get { return this.Value is bool ? (bool)this.Value : default(bool); } set { if (this.Value != null && this.Value.Equals(value)) return; this.RaisePropertyChanging(); if (this.IsClass) this.ToField(); this.Value = value; this.RaisePropertyChanged(); } }
        public int Count { get { if (!this.IsClass) return 0; return this.Children.Count; } }
        public virtual string Key
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
                nameList.Reverse();
                return string.Join("/", nameList.ToArray());
            }
        }

        public virtual int ParentCount { get { int i = 0; var cf = this; while (cf.Parent != null) { i++; cf = cf.Parent; } return i; } }

        protected ConfigField(bool ConfigReferenceConstructorCrapTrick) { }
        public ConfigField(string name)
        {
            this.Parent = default(ConfigField);
            this.Value = "";
            this._ConfigParentName = default(string);
            this._Name = name;
        }
        public ConfigField()
        {
            this.ToClass();
            this.Value = "";
            this.Parent = default(ConfigField);
            this._ConfigParentName = default(string);
            this._Name = string.Empty;
        }


        /// <summary>
        /// Swaps the position of given keys
        /// </summary>
        /// <param name="keyA">Key to swap in this direct child</param>
        /// <param name="keyB">Key to swap in this direct child</param>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="ArgumentException"/>
        public void SwapKeyIndexies(string keyA, string keyB)
        {
            if (!this.IsClass)
                throw new InvalidOperationException(EX_INVALIDTYPE_CLASS);
            if (!IsValidKey(keyA) || !IsValidKey(keyB))
                throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
            if (!this.Contains(keyA) || !this.Contains(keyB))
                throw new ArgumentException(EX_INVALIDARG_KEYNOTFOUND);
            int index = -1;
            for (int i = 0; i < this.Count; i++)
            {
                if (this.Children[i].Name.Equals(keyA, StringComparison.InvariantCultureIgnoreCase) || this.Children[i].Name.Equals(keyB, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (index == -1)
                    {
                        index = i;
                    }
                    else
                    {
                        this.RaisePropertyChanging("Children");
                        var tmp = this.Children[i];
                        this.Children[i] = this.Children[index];
                        this.Children[index] = tmp;
                        this.RaisePropertyChanged("Children");
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new empty <see cref="ConfigField"/> with given key into this class.
        /// Requires this <see cref="ConfigField"/> to be a class!
        /// Sub-Keys separated by "/" are not allowed
        /// </summary>
        /// <param name="key">Non-Existing key for the new field</param>
        /// <param name="parent">either default(string) or the parents class name</param>
        /// <returns>new <see cref="ConfigField"/></returns>
        /// <exception cref="ArgumentException"/>
        public ConfigField AddKey(string key, string parent = default(string))
        {
            if (!ConfigField.IsValidKey(key))
                throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
            if (!this.IsArray && !this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASSARRAY);
            if (this.Contains(key))
                throw new ArgumentException(EX_INVALIDARG_KEYALREADYEXISTS);
            if (!this.IsClass)
                this.ToClass();
            this.RaisePropertyChanging("Children");
            ConfigField field = new ConfigField(key);
            field.Parent = this;
            if(!string.IsNullOrWhiteSpace(parent))
            {
                field.ConfigParentName = parent;
                field.ToClass();
            }
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
        public ConfigField GetKey(string key, KeyMode mode)
        {
            if (!this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASS);
            if (key.Contains('/'))
            {
                //Key-path was provided
                bool createReference = false;
                var keys = key.Split('/');
                ConfigField currentField = this;
                for (var i = 0; i < keys.Length; i++)
                {
                    var it = keys[i];
                    if (string.IsNullOrWhiteSpace(it))
                        continue;
                    ConfigField tmp;
                    try
                    {
                        tmp = currentField.GetKey(it, KeyMode.NullOnNotFound);
                    }
                    catch
                    {
                        tmp = null;
                    }
                    if (tmp != null)
                    {
                        currentField = tmp;
                    }
                    else
                    {
                        switch (mode)
                        {
                            case KeyMode.CheckParentsThrow:
                            case KeyMode.CheckParentsNull:
                                if (string.IsNullOrWhiteSpace(currentField.ConfigParentName))
                                {
                                    if (mode == KeyMode.CheckParentsNull)
                                        return null;
                                    else
                                        throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND, key);
                                }
                                else
                                {
                                    StringBuilder builder = new StringBuilder();
                                    createReference = true;
                                    currentField = currentField.FindConfigKeyInHirarchy(currentField.ConfigParentName).GetKey(string.Join("/", keys.GetRange(i)), mode);
                                    if (currentField == null)
                                        return null;
                                    i = keys.Length;
                                }
                                break;
                            case KeyMode.CreateNew:
                                currentField = currentField.AddKey(it);
                                currentField.ToClass();
                                break;
                            case KeyMode.NullOnNotFound:
                                return null;
                            case KeyMode.EmptyReferenceOnNotFound:
                                return new ConfigFieldReference(key, currentField);
                            case KeyMode.HighestMatchAvailable:
                                return currentField;
                            default:
                                throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND, string.Join("/", keys.GetRange(i)));
                        }
                    }
                }
                if (createReference)
                {
                    if (currentField is ConfigFieldReference)
                    {
                        currentField = (currentField as ConfigFieldReference).ReferencedConfigField;
                    }
                    if (this.ParentCount > 0)
                    {
                        string[] strArray = new string[this.ParentCount];
                        int index = strArray.Length - 1;
                        var curField = this;
                        while(curField != default(ConfigField) && index >= 0)
                        {
                            strArray[index] = curField.Name;
                            index--;
                            curField = curField.Parent;
                        }
                        return new ConfigFieldReference(currentField, string.Concat(string.Join("/", strArray), '/', key.TrimStart('/')));
                    }
                    else
                    {
                        return new ConfigFieldReference(currentField, key);
                    }
                }
                else
                {
                    return currentField;
                }
            }
            else
            {
                //Single key was provided
                if (mode == KeyMode.EmptyReferenceOnNotFound)
                    throw new ArgumentException(EX_INVALIDARG_INVALIDKEYMODE_SINGLEKEY);
                if (mode == KeyMode.HighestMatchAvailable)
                    throw new ArgumentException(EX_INVALIDARG_INVALIDKEYMODE_SINGLEKEY);
                if (!ConfigField.IsValidKey(key))
                    throw new ArgumentException(EX_INVALIDARG_INVALIDKEY);
                foreach (var it in this.Children)
                {
                    if (it.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        return it;
                }
                switch (mode)
                {
                    case KeyMode.CheckParentsThrow:
                    case KeyMode.CheckParentsNull:
                        if (string.IsNullOrWhiteSpace(this.ConfigParentName))
                        {
                            if (mode == KeyMode.CheckParentsNull)
                                return null;
                            else
                                throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND, key);
                        }
                        else
                        {
                            return this.FindConfigKeyInHirarchy(this.ConfigParentName)[key];
                        }
                    case KeyMode.CreateNew:
                        var field = this.AddKey(key);
                        field.ToClass();
                        return field;
                    case KeyMode.NullOnNotFound:
                        return null;
                    default:
                        throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND, key);
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
            if (!this.IsArray && !this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASSARRAY);
            if (!this.IsClass)
                this.ToClass();
            ConfigField field = this.GetKey(key, KeyMode.CreateNew);
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
        /// <param name="key">key to remove, sub-keys can be separated by "/"</param>
        /// <exception cref="ArgumentException"/>
        public void RemoveKey(string key)
        {
            if (!this.IsClass)
                throw new ArgumentException(EX_INVALIDTYPE_CLASS);
            var field = this.GetKey(key, KeyMode.NullOnNotFound);
            if (field == null)
                throw new ArgumentException(EX_INVALIDARG_KEYNOTFOUND);
            field.Parent.RaisePropertyChanging("Children");
            field.Parent.Children.Remove(field);
            field.Parent.RaisePropertyChanged("Children");
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
        public ConfigField this[string key] { get { return this.GetKey(key, KeyMode.CheckParentsThrow); } }
        /// <summary>
        /// <para>Receives <see cref="ConfigField"/>s with given index & length from this class.
        /// Requires this <see cref="ConfigField"/> to be a class!</para>
        /// </summary>
        /// <param name="index">Index to receive</param>
        /// <param name="length">Ammount of fields to receive</param>
        /// <returns><see cref="ConfigField"/> with given index</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ConfigField[] this[int index, int length]
        {
            get
            {
                if (!this.IsClass)
                    throw new ArgumentException(EX_INVALIDTYPE_CLASS);
                if (index < 0 || this.Children.Count <= index + length)
                    throw new ArgumentOutOfRangeException();
                List<ConfigField> list = new List<ConfigField>(length);
                for(int i = index; i < index + length; i++)
                {
                    list.Add(this.Children[i]);
                }
                return list.ToArray();
            }
        }
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
            this.Value = "";
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
            return this.GetKey(key, KeyMode.NullOnNotFound) != null;
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
        /// Converts value to write-to-file comform string representation
        /// </summary>
        /// <returns>Write-Out ready string representation of this value</returns>
        /// <exception cref="ArgumentException"/>
        public string ToValueString()
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
                builder.Append(val ? "true" : "false");
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
        /// Creates a formatted string which represents this <see cref="ConfigField"/> in proper ArmA way.
        /// </summary>
        /// <param name="tabCount">Tab Count this print should have</param>
        /// <returns>Formatted string containing only this <see cref="ConfigField"/></returns>
        public string ToPrintString(int tabCount = 0)
        {
            StringBuilder builder = new StringBuilder();

            if (this.IsClass)
            {
                builder.Append(new string('\t', tabCount));
                builder.Append("class ");
                builder.Append(this.Name);
                if (!string.IsNullOrWhiteSpace(this.ConfigParentName))
                {
                    builder.Append(" : ");
                    builder.Append(this.ConfigParentName);
                }
                builder.Append("\r\n");
                builder.Append(new string('\t', tabCount));
                builder.AppendLine("{");
                foreach(var it in this.Children)
                {
                    if(!it.IsClass)
                        builder.Append(new string('\t', tabCount + 1));
                    builder.AppendLine(it.ToPrintString(tabCount + 1));
                }
                builder.Append(new string('\t', tabCount));
                builder.Append("};");
            }
            else
            {
                builder.Append(this.Name);
                if (this.IsArray)
                {
                    builder.Append("[]");
                }
                builder.Append(" = ");
                builder.Append(this.ToValueString());
                builder.Append(';');
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
        /// <summary>
        /// Returns this <see cref="ConfigField"/>s enumerator
        /// </summary>
        /// <returns><see cref="ConfigFieldEnumerator"/> containing the different children of this <see cref="ConfigField"/></returns>
        public ConfigFieldEnumerator GetEnumerator()
        {
            return new ConfigFieldEnumerator(this);
        }
        /// <summary>
        /// Returns this <see cref="ConfigField"/>s enumerator
        /// </summary>
        /// <returns><see cref="ConfigFieldEnumerator"/> containing the different children of this <see cref="ConfigField"/></returns>
        IEnumerator<ConfigField> IEnumerable<ConfigField>.GetEnumerator()
        {
            return new ConfigFieldEnumerator(this);
        }
        /// <summary>
        /// Returns this <see cref="ConfigField"/>s enumerator
        /// </summary>
        /// <returns><see cref="ConfigFieldEnumerator"/> containing the different children of this <see cref="ConfigField"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ConfigFieldEnumerator(this);
        }
        /// <summary>
        /// Returns a deep key enumerator (<see cref="List{String}"/>)
        /// </summary>
        /// <param name="headerFirst">Determines if header key should come before or after the children</param>
        /// <returns><see cref="List{String}"/> containing the keys</returns>
        public IEnumerable<string> GetEnumeratorDeep(bool headerFirst = true)
        {

            var keys = new List<string>();
            if (!this.IsClass)
                return keys;
            foreach (var it in this.Children)
            {
                if (headerFirst)
                {
                    keys.Add(it.Key);
                    keys.AddRange(it.GetEnumeratorDeep(headerFirst));
                }
                else
                {
                    keys.AddRange(it.GetEnumeratorDeep(headerFirst));
                    keys.Add(it.Key);
                }
            }
            return keys;
        }


        public override string ToString()
        {
            
            if(this.IsClass)
            {
                return string.Format("{0}", this.Name);
            }
            else
            {
                var val = this.ToValueString();
                val = string.Format("{0}{1} = {2}", this.Name, this.IsArray ? "[]" : string.Empty, val);
                return val.Length > 64 ? string.Format("{0}...", val.Substring(0, 61)) : val;
            }
        }
    }
}
