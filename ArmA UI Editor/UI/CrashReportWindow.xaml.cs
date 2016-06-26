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
using System.Windows.Shapes;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for CrashReportWindow.xaml
    /// </summary>
    public partial class CrashReportWindow : Window
    {
        public string ReportText { get; private set; }
        public CrashReportWindow()
        {
            InitializeComponent();
            ReportText = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReportText = textbox.Text;
            this.DialogResult = true;
            e.Handled = true;
        }
    }
}
