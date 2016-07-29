using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ArmA_UI_Editor.Code.Markup
{
    public abstract class BindConfig : MarkupExtension
    {
        public string Path { get; set; }
        public static string CurrentClassPath { get; set; }
        public static string AddInPath { get; set; }

        public BindConfig() { }
    }
}
