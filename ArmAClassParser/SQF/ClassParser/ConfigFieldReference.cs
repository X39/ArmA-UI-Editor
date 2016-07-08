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
        internal bool IsReference;

        public override ConfigField Parent { get { return this.ReferencedConfigField.Parent; } }
        public override string Name { get { return this.ReferencedConfigField.Name; } set { this.ReferencedConfigField.Name = value; } }
        public override string ConfigParentName { get { return this.ReferencedConfigField.ConfigParentName; } set { this.ReferencedConfigField.ConfigParentName = value; } }
        public override ITextBuffer ThisBuffer { get { return this.ReferencedConfigField.ThisBuffer; } }
        public override object Value { get { return this.ReferencedConfigField.Value; } internal set { if (this.IsReference) this.CreateField(); this.ReferencedConfigField.Value = value; } }
        private string _Key;
        public override string Key { get { return this._Key; } }
        public override int ParentCount { get { return _Key.Split('/').Count((s) => string.IsNullOrWhiteSpace(s)); } }


        public ConfigFieldReference(ConfigField referencedField, string thisKey) : base()
        {
            this.IsReference = true;
            this.ReferencedConfigField = referencedField;
            this.Marks = referencedField.Marks;
            this._Key = thisKey;
        }
        private void CreateField()
        {
            if (!this.IsReference)
                return;
            ConfigField field = this.TreeRoot;

            field = field.GetKey(this.Key, KeyMode.CreateNew);
            this.ReferencedConfigField = field;
            this.Marks = this.ReferencedConfigField.Marks;
            this.IsReference = false;
        }
    }
}
