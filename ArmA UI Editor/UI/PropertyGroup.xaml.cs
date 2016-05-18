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

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for PropertyGroup.xaml
    /// </summary>
    public partial class PropertyGroup : UserControl
    {
        public static readonly DependencyProperty IsExpanedProperty = DependencyProperty.Register("IsExpaned", typeof(bool), typeof(PropertyGroup));
        public bool IsExpaned { get { return (bool)GetValue(IsExpanedProperty); } set { SetValue(IsExpanedProperty, value); } }

        public PropertyGroup()
        {
            InitializeComponent();
            this.IsExpaned = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsExpaned = !this.IsExpaned;
        }
    }
}
