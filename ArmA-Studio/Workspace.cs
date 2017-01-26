using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI_Editor
{
    public sealed class Workspace
    {
        private static Workspace _CurrentWorkspace;
        public static Workspace CurrentWorkspace { get { return _CurrentWorkspace; } set { if (_CurrentWorkspace != null) _CurrentWorkspace.Close(); _CurrentWorkspace = value; value.Open();  } }

        private void Open()
        {
            throw new NotImplementedException();
        }

        private void Close()
        {
            throw new NotImplementedException();
        }
    }
}
