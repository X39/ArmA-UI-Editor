using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmAClassParser.SQF.ClassParser
{
    public class ConfigField : INotifyPropertyChanged, INotifyPropertyChanging
    {
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
        #region ExceptionStrings
        private const string EX_INVALIDTYPE_ARRAY = "ConfigField is not of type Array";
        private const string EX_INVALIDARG_KEYALREADYEXISTS = "ConfigField already contains provided key";
        private const string EX_INVALIDARG_KEYNOTFOUND = "ConfigField does not contains provided key";
        private const string EX_INVALIDARG_INVALIDKEY = "Provided key contains unallowed characters. Only AlphaNumeric characters and '_' are allowed.";
        private const string EX_INVALIDARG_FIELDNOTFOUND = "ConfigField does not contains provided field";
        private const string EX_INVALIDOPS_ALREADYARRAY = "ConfigField is already an Array";
        #endregion

        private object _value;

        public object Value { get { return _value; } set { this.RaisePropertyChanging(); _value = value; this.RaisePropertyChanged(); } }
        public bool IsArray { get { return this.Value is List<ConfigField>; } }
        private List<ConfigField> Children { get { return this.Value as List<ConfigField>; } set { this.Value = value; } }

        public bool IsNumber { get { return this.Value is double; } }
        public double Number { get { return this.Value is double ? (double)this.Value : default(double); } set { this.Value = value; } }
        public bool IsString { get { return this.Value is string; } }
        public string String { get { return this.Value is string ? (string)this.Value : default(string); } set { this.Value = value; } }
        public bool IsBoolean { get { return this.Value is bool; } }
        public bool Boolean { get { return this.Value is bool ? (bool)this.Value : default(bool); } set { this.Value = value; } }

        public ConfigField Parent { get; private set; }
        public string Name { get; set; }




        public ConfigField(string name)
        {
            this.Parent = default(ConfigField);
            this._value = null;
            this.Name = name;
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

            ConfigField field = new ConfigField(key);
            field.Parent = this;
            this.RaisePropertyChanging();
            this.Children.Add(field);
            this.RaisePropertyChanged();
            return field;
        }
        /// <summary>
        /// Removes given <see cref="ConfigField"/> from this array.
        /// Requires this <see cref="ConfigField"/> to be an array!
        /// </summary>
        /// <param name="field"></param>
        /// <exception cref="ArgumentException"/>
        public void RemoveField(ConfigField field)
        {
            if (!this.IsArray)
                throw new ArgumentException(EX_INVALIDTYPE_ARRAY);
            if (!this.Children.Contains(field))
                throw new ArgumentException(EX_INVALIDARG_FIELDNOTFOUND);

            this.RaisePropertyChanging();
            this.Children.Remove(field);
            this.RaisePropertyChanged();
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
        /// </summary>
        /// <param name="key">key to receive</param>
        /// <returns><see cref="ConfigField"/> with given key</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="KeyNotFoundException"/>
        public ConfigField this[string key]
        {
            get
            {
                if (!this.IsArray)
                    throw new ArgumentException(EX_INVALIDTYPE_ARRAY);
                if (key.Contains('/'))
                {
                    //Key-path was provided
                    var keys = key.Split('/');
                    ConfigField currentField = this;
                    foreach(var it in keys)
                    {
                        if (string.IsNullOrWhiteSpace(it))
                            continue;
                        currentField = currentField[it];
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
                    throw new KeyNotFoundException(EX_INVALIDARG_KEYNOTFOUND);
                }
            }
        }
        /// <summary>
        /// Changes this <see cref="ConfigField"/> to an array.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public void ToArray()
        {
            if (this.IsArray)
                throw new InvalidOperationException(EX_INVALIDOPS_ALREADYARRAY);
            this.Children = new List<ConfigField>();
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
            if (!this.IsArray)
                throw new ArgumentException(EX_INVALIDTYPE_ARRAY);

            foreach (var it in this.Children)
            {
                if (it.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool IsValidKey(string key)
        {
            return key.All((c) => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}
