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
using System.Windows.Markup;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for Property.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class Property : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(Property));
        public string Header { get { return (string)GetValue(HeaderProperty); } set { SetValue(HeaderProperty, value); } }

        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly("Children", typeof(UIElementCollection), typeof(Property), new PropertyMetadata());
        public UIElementCollection Children { get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); } private set { SetValue(ChildrenProperty, value); } }

        public Property()
        {
            InitializeComponent();
            this.Children = Presenter.Children;
        }
    }
}
