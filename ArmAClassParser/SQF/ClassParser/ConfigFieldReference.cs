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
    /// <summary>
    /// Special wrapper class for <see cref="ConfigField"/>
    /// </summary>
    public class ConfigFieldReference : ConfigField
    {
        internal ConfigField ReferencedConfigField;
        internal ConfigField ParentalReference;
        internal bool IsReference;

        public override ConfigField Parent { get { return this.ReferencedConfigField.Parent; } }
        public override string Name { get { return this.ReferencedConfigField.Name; } set { this.ReferencedConfigField.Name = value; } }
        public override string ConfigParentName { get { return this.ReferencedConfigField.ConfigParentName; } set { this.ReferencedConfigField.ConfigParentName = value; } }
        public override object Value { get { return this.ReferencedConfigField == null ? null : this.ReferencedConfigField.Value; } set { if (this.IsReference) this.CreateField(); this.ReferencedConfigField.Value = value; } }
        private string _Key;
        public override string Key { get { return this._Key; } }
        public override int ParentCount { get { return _Key.Split('/').Count((s) => string.IsNullOrWhiteSpace(s)); } }


        public ConfigFieldReference(ConfigField referencedField, string thisKey) : base(true)
        {
            this.IsReference = true;
            this.ReferencedConfigField = referencedField;
            this.ParentalReference = null;
            this._Key = thisKey;
        }
        public ConfigFieldReference(string thisKey, ConfigField parent) : base(true)
        {
            this.IsReference = true;
            this.ReferencedConfigField = null;
            this.ParentalReference = parent;
            this._Key = thisKey;
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Children")
                return;
            var field = sender as ConfigField;
            field = field.GetKey(this.Key.Remove(0, field.Key.Length), KeyMode.HighestMatchAvailable);
            this.ParentalReference.PropertyChanged -= Parent_PropertyChanged;
            if (field.Key == this.Key)
            {
                this.ParentalReference = null;
                this.IsReference = false;
                this.ReferencedConfigField = field;
            }  
            else
            {
                this.ParentalReference = field;
            }
        }

        private void CreateField()
        {
            if (!this.IsReference)
                return;
            ConfigField field = null;
            if (this.ReferencedConfigField != null)
            {
                field = this.ReferencedConfigField.TreeRoot;
            }
            if (this.ParentalReference != null)
            {
                field = this.ParentalReference.TreeRoot;
                this.ParentalReference = null;
            }

            field = field.GetKey(this.Key, KeyMode.CreateNew);
            this.ReferencedConfigField = field;
            this.IsReference = false;
        }
    }
}
