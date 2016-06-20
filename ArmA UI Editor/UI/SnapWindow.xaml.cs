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

        public static readonly DependencyProperty HeaderVisibleProperty = DependencyProperty.Register("HeaderVisible", typeof(bool), typeof(SnapWindow));
        public bool HeaderVisible { get { return (bool)GetValue(HeaderVisibleProperty); } set { SetValue(HeaderVisibleProperty, value); } }

        internal Code.Interface.ISnapWindow Window { get { return this.WindowContent as Code.Interface.ISnapWindow; } }
        public static readonly DependencyProperty WindowProperty = DependencyProperty.Register("Window", typeof(object), typeof(SnapWindow));
        public object WindowContent { get { return GetValue(WindowProperty); } set { SetValue(WindowProperty, value); } }

        public SnapWindow()
        {
            InitializeComponent();
            Pinned = false;
            HeaderVisible = true;
        }
        internal SnapWindow(Code.Interface.ISnapWindow Window, string HeaderText)
        {
            InitializeComponent();
            Pinned = false;
            HeaderVisible = true;

            this.WindowContent = Window;
            this.Header.Text = HeaderText;
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
