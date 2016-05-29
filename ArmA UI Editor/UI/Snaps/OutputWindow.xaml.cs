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

namespace ArmA_UI_Editor.UI.Snaps
{
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : Page, Code.Interface.ISnapWindow
    {
        public OutputWindow()
        {
            InitializeComponent();
        }

        public void LoadSnap()
        {
            Logger.Instance.OnLog += Logger_OnLog;
        }

        public void UnloadSnap()
        {
            Logger.Instance.OnLog -= Logger_OnLog;
        }

        private void Logger_OnLog(object sender, string e)
        {
            this.LogBox.Text += e + '\n';
        }
    }
}
