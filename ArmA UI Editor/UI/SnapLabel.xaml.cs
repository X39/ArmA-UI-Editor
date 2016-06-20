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
    /// Interaction logic for SnapLabel.xaml
    /// </summary>
    public partial class SnapLabel : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SnapLabel));
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        public static readonly DependencyProperty DockedAtProperty = DependencyProperty.Register("DockedAt", typeof(Dock), typeof(SnapLabel));
        public Dock DockedAt { get { return (Dock)GetValue(DockedAtProperty); } set { SetValue(DockedAtProperty, value); } }

        public static readonly DependencyProperty IsDisplayedProperty = DependencyProperty.Register("IsDisplayed", typeof(bool), typeof(SnapLabel));
        public bool IsDisplayed { get { return (bool)GetValue(IsDisplayedProperty); } set { SetValue(IsDisplayedProperty, value); } }

        public SnapLabel()
        {
            InitializeComponent();
            DockedAt = Dock.Top;
            IsDisplayed = false;
        }
    }
}
