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
    public class ConfigFieldEnumerator : IEnumerator<object>
    {
        private ConfigField Field;
        private int Index;

        internal ConfigFieldEnumerator(ConfigField field)
        {
            this.Field = field;
            this.Index = -1;
        }
        public object Current
        {
            get
            {
                if (this.Index < 0)
                    return default(object);
                return this.Field[this.Index];
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return ++this.Index >= Field.Count;
        }

        public void Reset()
        {
            this.Index = -1;
        }
    }
}
