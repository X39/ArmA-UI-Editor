using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.ComponentModel;

namespace ArmA.Studio.Dialogs
{
    public struct FileType
    {
        public string Image { get; set; }
        public string FileTypeName { get; set; }
        public string DefaultContent { get; set; }
        public string Extension { get; set; }
    }
}
