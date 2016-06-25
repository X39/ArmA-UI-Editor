
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArmA_UI_Editor.Code.AddInUtil;
using SQF.ClassParser;
using ArmA_UI_Editor.Code;

namespace ArmA_UI_Editor.UI.Snaps
{
    public partial class EventsSnap : Page, Code.Interface.ISnapWindow
    {
        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Right; } }

        public EventsSnap()
        {
            InitializeComponent();
        }
        public void UnloadSnap()
        {

        }
        public void LoadSnap()
        {

        }
    }
}
