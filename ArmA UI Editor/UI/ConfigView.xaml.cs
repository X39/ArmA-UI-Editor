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
using ArmA_UI_Editor.Code;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : Window
    {
        public ConfigView()
        {
            InitializeComponent();
        }

        private void TreeView_Initialized(object sender, EventArgs e)
        {
            TreeView tv = sender as TreeView;
            tv.ItemsSource = Code.AddInManager.Instance.MainFile;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tv = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeView;
            Clipboard.SetText((tv.SelectedItem as SQF.ClassParser.ConfigField).ToPrintString());
        }

        private void TreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tvi = (e.OriginalSource as TextBlock).FindInVisualTreeUpward<TreeViewItem>();
            if(tvi == null)
            {
                e.Handled = true;
                return;
            }
            tvi.Focus();
        }
    }
}
