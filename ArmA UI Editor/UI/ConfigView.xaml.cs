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
    }
}
