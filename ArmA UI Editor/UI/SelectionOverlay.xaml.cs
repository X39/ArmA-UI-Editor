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
using ArmA_UI_Editor.Code;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for SelectionOverla.xaml
    /// </summary>
    public partial class SelectionOverlay : UserControl
    {
        public class MoveEventArgs : EventArgs
        {
            public double DeltaX { get; private set; }
            public double DeltaY { get; private set; }
            public UIElement Element { get; private set; }
            public MoveEventArgs(double deltaX, double deltaY, UIElement element)
            {
                this.DeltaX = deltaX;
                this.DeltaY = deltaY;
                this.Element = element;
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

        public event EventHandler<System.Windows.Input.MouseEventArgs> OnStartMove;
        public event EventHandler<System.Windows.Input.MouseEventArgs> OnStopMove;


        public List<UIElement> ToggledElements;



        public SelectionOverlay()
        {
            ToggledElements = new List<UIElement>();
            MoveState = MoveStateEnum.PREPARE;
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

        private enum MoveStateEnum
        {
            NONE,
            PREPARE,
            MOVING
        }
        private MoveStateEnum MoveState;
        private void Rectangle_Center_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoveState = MoveStateEnum.NONE;
            if (this.OnStopMove != null)
            {
                this.OnStopMove(this, e);
                e.Handled = true;
            }
        }
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (MoveState == MoveStateEnum.PREPARE && this.OnStartMove != null)
            {
                this.OnStartMove(this, e);
                MoveState = MoveStateEnum.MOVING;
                e.Handled = true;
            }
        }
        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (MoveState == MoveStateEnum.PREPARE)
                MoveState = MoveStateEnum.NONE;
        }
        public void DoMove(double deltaX, double deltaY)
        {
            foreach(var it in this.ToggledElements)
            {
                if (this.OnElementMove != null)
                    this.OnElementMove(this, new MoveEventArgs(deltaX, deltaY, it));
            }
            var rect = this.GetCanvasMetrics();
            rect.X += deltaX;
            rect.Y += deltaY;
            this.SetCanvasMetrics(rect);
        }

        /// <summary>
        /// Toggles given element inside of this SelectionOverlay
        /// </summary>
        /// <param name="thisElement">Element to toggle</param>
        /// <returns>true in case this was not the last element and thus selectionOverlay should continue to exist, false if it was the last one</returns>
        internal bool ToggleElement(UIElement thisElement)
        {
            if (this.ToggledElements.Contains(thisElement))
            {
                this.ToggledElements.Remove(thisElement);
            }
            else
            {
                this.ToggledElements.Add(thisElement);
            }
            if (this.ToggledElements.Count == 0)
                return false;

            double MinLeft = double.PositiveInfinity;
            double MinTop = double.PositiveInfinity;
            double MaxRight = double.NegativeInfinity;
            double MaxBottom = double.NegativeInfinity;
            foreach (var it in this.ToggledElements)
            {
                var itMetrics = it.GetCanvasMetrics();
                if (MinLeft > itMetrics.Left)
                {
                    MinLeft = itMetrics.Left;
                }
                if (MinTop > itMetrics.Top)
                {
                    MinTop = itMetrics.Top;
                }
                if (MaxRight < itMetrics.Right)
                {
                    MaxRight = itMetrics.Right;
                }
                if (MaxBottom < itMetrics.Bottom)
                {
                    MaxBottom = itMetrics.Bottom;
                }
            }
            Canvas.SetLeft(this, MinLeft);
            Canvas.SetTop(this, MinTop);
            Canvas.SetRight(this, MaxRight);
            Canvas.SetBottom(this, MaxBottom);
            this.Width = MaxRight - MinLeft;
            this.Height = MaxBottom - MinTop;

            return true;
        }
    }
}
