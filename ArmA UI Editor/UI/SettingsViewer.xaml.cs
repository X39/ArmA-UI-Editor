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
    /// Interaction logic for SettingsViewer.xaml
    /// </summary>
    public partial class SettingsViewer : Window
    {
        public SettingsViewer()
        {
            InitializeComponent();
        }

        private void btn_privacy_Click(object sender, RoutedEventArgs e)
        {
            this.ContentFrame.Content = this.Resources["General_Privacy"] as Page;
        }
        private void btn_updates_Click(object sender, RoutedEventArgs e)
        {
            this.ContentFrame.Content = this.Resources["General_Updates"] as Page;
        }
    }
}
