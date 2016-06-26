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
        public static readonly DependencyProperty PullThicknessProperty = DependencyProperty.Register("PullThickness", typeof(int), typeof(SelectionOverlay));
        public int PullThickness { get { return (int)GetValue(PullThicknessProperty); } set { SetValue(PullThicknessProperty, value); } }

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
            public double DeltaX { get; private set; }
            public double DeltaY { get; private set; }
            public UIElement Element { get; private set; }
            public ResizeEventArgs(Direction dir, double deltaX, double deltaY, UIElement element)
            {
                this.Dir = dir;
                this.DeltaX = deltaX;
                this.DeltaY = deltaY;
                this.Element = element;
            }
        }
        private enum MoveStateEnum
        {
            NONE,
            PREPARE,
            MOVING
        }
        private MoveStateEnum MoveState;
        private ResizeEventArgs.Direction? ResizeDirection;
        public event EventHandler<MoveEventArgs> OnElementMove;
        public event EventHandler<ResizeEventArgs> OnElementResize;
        public event EventHandler<FrameworkElement[]> OnOperationFinalized;

        public event EventHandler<System.Windows.Input.MouseEventArgs> OnStartMove;
        public event EventHandler<EventArgs> OnStopMove;


        public List<FrameworkElement> ToggledElements;
        internal static readonly int DefaultThickness = 5;

        public SelectionOverlay(bool mouseDownOnCreate = true)
        {
            InitializeComponent();
            ToggledElements = new List<FrameworkElement>();
            if(mouseDownOnCreate)
                MoveState = MoveStateEnum.PREPARE;
            ResizeDirection = null;
            PullThickness = 5;
        }
        
        private void Rectangle_TopLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.TopLeft;
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Rectangle_BotLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.BotLeft;
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Rectangle_TopRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.TopRight;
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Rectangle_BotRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.BotRight;
            MoveState = MoveStateEnum.PREPARE;
        }

        private void Rectangle_Left_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.Left;
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Rectangle_Top_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.Top;
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Rectangle_Right_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.Right;
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Rectangle_Bot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeDirection = ResizeEventArgs.Direction.Bot;
            MoveState = MoveStateEnum.PREPARE;
        }

        private void Rectangle_Center_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveState = MoveStateEnum.PREPARE;
        }
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMove();
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
        public void DoMove(double deltaX, double deltaY)
        {
            Rect rect = this.GetCanvasMetrics();
            Rect origRect = rect;
            bool isTopLeftMovement = false;
            if (this.ResizeDirection.HasValue)
            {
                isTopLeftMovement = new SelectionOverlay.ResizeEventArgs.Direction[] {
                        SelectionOverlay.ResizeEventArgs.Direction.TopRight,
                        SelectionOverlay.ResizeEventArgs.Direction.Top,
                        SelectionOverlay.ResizeEventArgs.Direction.TopLeft,
                        SelectionOverlay.ResizeEventArgs.Direction.Left,
                        SelectionOverlay.ResizeEventArgs.Direction.BotLeft
                    }.Contains(this.ResizeDirection.Value); ;
                switch (this.ResizeDirection.Value)
                {
                    case ResizeEventArgs.Direction.Top:
                    case ResizeEventArgs.Direction.Bot:
                        deltaX = 0;
                        break;
                    case ResizeEventArgs.Direction.Right:
                    case ResizeEventArgs.Direction.Left:
                        deltaY = 0;
                        break;
                }
                deltaX = isTopLeftMovement ? -deltaX : deltaX;
                deltaY = isTopLeftMovement ? -deltaY : deltaY;
                if (isTopLeftMovement)
                {
                    rect.X -= deltaX;
                    rect.Y -= deltaY;
                }
                if (rect.Width + deltaX < 0 || rect.Height + deltaY < 0)
                {
                    return;
                }
                rect.Width += deltaX;
                rect.Height += deltaY;
                this.Width = rect.Width;
                this.Height = rect.Height;
                this.SetCanvasMetrics(rect);
            }
            else
            {
                rect.X += deltaX;
                rect.Y += deltaY;
                this.SetCanvasMetrics(rect);
            }
            foreach (var it in this.ToggledElements)
            {
                if(this.ResizeDirection.HasValue)
                {
                    var itSize = it.GetCanvasMetrics();
                    var localDeltaX = deltaX * (itSize.Width / origRect.Width);
                    var localDeltaY = deltaY * (itSize.Height / origRect.Height);
                    if (this.OnElementResize != null)
                        this.OnElementResize(this, new ResizeEventArgs(this.ResizeDirection.Value, localDeltaX, localDeltaY, it));
                    if (isTopLeftMovement)
                    {
                        bool conditionX = (localDeltaX != 0 && itSize.X + itSize.Width != origRect.X + origRect.Width);
                        bool conditionY = (localDeltaY != 0 && itSize.Y + itSize.Height != origRect.Y + origRect.Height);
                        if (conditionX || conditionY)
                        {
                            if (this.OnElementMove != null)
                                this.OnElementMove(this, new MoveEventArgs(conditionX ? -(deltaX - localDeltaX) : 0, conditionY ? -(deltaY - localDeltaY) : 0 , it));
                        }
                    }
                    else
                    {
                        bool conditionX = (localDeltaX != 0 && itSize.X != origRect.X);
                        bool conditionY = (localDeltaY != 0 && itSize.Y != origRect.Y);
                        if (conditionX || conditionY)
                        {
                            if (this.OnElementMove != null)
                                this.OnElementMove(this, new MoveEventArgs(conditionX ? deltaX - localDeltaX : 0, conditionY ? deltaY - localDeltaY : 0, it));
                        }
                    }
                }
                else
                {
                    if (this.OnElementMove != null)
                        this.OnElementMove(this, new MoveEventArgs(deltaX, deltaY, it));
                }
            }
        }
        public void UpdateMetrics()
        {

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
        }

        /// <summary>
        /// Toggles given element inside of this SelectionOverlay
        /// </summary>
        /// <param name="thisElement">Element to toggle</param>
        /// <returns>true in case this was not the last element and thus selectionOverlay should continue to exist, false if it was the last one</returns>
        public bool ToggleElement(FrameworkElement thisElement)
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
            UpdateMetrics();

            return true;
        }

        public void ReleaseMove()
        {
            if (MoveState != MoveStateEnum.NONE)
            {
                MoveState = MoveStateEnum.NONE;
                if (this.OnStopMove != null)
                {
                    this.OnStopMove(this, new EventArgs());
                }
                if(this.OnOperationFinalized != null)
                {
                    this.OnOperationFinalized(this, this.ToggledElements.ToArray());
                }
                this.ResizeDirection = null;
            }
        }
    }
}
