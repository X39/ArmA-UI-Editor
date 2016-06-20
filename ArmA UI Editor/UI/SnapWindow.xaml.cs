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
    /// Interaction logic for SnapWindow.xaml
    /// </summary>
    public partial class SnapWindow : UserControl
    {
        public static readonly DependencyProperty PinnedProperty = DependencyProperty.Register("Pinned", typeof(bool), typeof(SnapWindow));
        public bool Pinned { get { return (bool)GetValue(PinnedProperty); } set { SetValue(PinnedProperty, value); } }

        internal Code.Interface.ISnapWindow Window { get { return this.ContentFrame.Content as Code.Interface.ISnapWindow; } }

        public SnapWindow()
        {
            InitializeComponent();
            Pinned = false;
        }

        private void PinWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Pinned = !this.Pinned;
        }
        public bool IsDisplayed
        {
            get
            {
                return this.Visibility == Visibility.Visible;
            }
        }
        public void DisplaySnap(Frame frame)
        {
            frame.Content = this;
            this.Visibility = Visibility.Visible;
        }

        public void HideSnap()
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
