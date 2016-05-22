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
    /// Interaction logic for SelectionOverla.xaml
    /// </summary>
    public partial class SelectionOverlay : UserControl
    {
        public class MoveEventArgs : EventArgs
        {
            public int DeltaX { get; private set; }
            public int DeltaY { get; private set; }
            public MoveEventArgs(int deltaX, int deltaY)
            {
                this.DeltaX = deltaX;
                this.DeltaY = deltaY;
            }
        }
        public class ResizeEventArgs : EventArgs
        {
            public enum Direction
            {
                TopLeft,
                Top,
                TopRight,
                Right,
                BotRight,
                Bot,
                BotLeft,
                Left
            }
            public Direction Dir { get; private set; }
            public int DeltaX { get; private set; }
            public int DeltaY { get; private set; }
            public ResizeEventArgs(Direction dir, int deltaX, int deltaY)
            {
                this.Dir = dir;
                this.DeltaX = deltaX;
                this.DeltaY = deltaY;
            }
        }
        public event EventHandler<MoveEventArgs> OnElementMove;
        public event EventHandler<ResizeEventArgs> OnElementResize;

        public event EventHandler<System.Windows.Input.MouseEventArgs> MouseMoved;



        public SelectionOverlay()
        {
            InitializeComponent();
        }

        private void Rectangle_TopLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        private void Rectangle_BotLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void Rectangle_TopRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void Rectangle_BotRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Rectangle_Left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void Rectangle_Top_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void Rectangle_Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void Rectangle_Bot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Rectangle_Center_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
