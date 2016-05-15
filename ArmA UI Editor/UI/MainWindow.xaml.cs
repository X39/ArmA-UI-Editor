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
using System.IO;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("class NewArmAUI");
            sb.AppendLine("{");
            sb.AppendLine(" idd = -1;");
            sb.AppendLine(" onLoad = \"\";");
            sb.AppendLine(" onUnload = \"\";");
            sb.AppendLine(" duration = 32000;");
            sb.AppendLine(" fadeIn = 0;");
            sb.AppendLine(" fadeOut = 0;");
            sb.AppendLine(" enableSimulation = 1;");
            sb.AppendLine(" ");
            sb.AppendLine(" class controls");
            sb.AppendLine(" {");
            sb.AppendLine("     ");
            sb.AppendLine(" }");
            sb.AppendLine("}");
            this.ConfigTextbox.Text = sb.ToString();
        }

        private void ListView_Initialized(object sender, EventArgs e)
        {
            ListView lw = sender as ListView;
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
        }

        private void ConfigTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
            }
            catch(Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
            
        }

        private void ToolBox_Initialized(object sender, EventArgs e)
        {
            foreach(var addIn in Code.AddInManager.Instance.AddIns)
            {
                foreach(var it in addIn.Files)
                {
                    this.ToolBox.Items.Add(it);
                }
            }
        }
    }
}
