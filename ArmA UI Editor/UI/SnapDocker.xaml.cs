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
    /// Interaction logic for SnapDocker.xaml
    /// </summary>
    public partial class SnapDocker : Page
    {
        private class TAG_Label
        {
            public SnapWindow window;
        }
        public SnapDocker()
        {
            InitializeComponent();
        }
        public void AddSnap(SnapWindow window, Dock dock)
        {
            StackPanel panel;
            switch (dock)
            {
                default:
                    throw new ArgumentException();
                case Dock.Top:
                    panel = SP_Top;
                    break;
                case Dock.Left:
                    panel = SP_Left;
                    break;
                case Dock.Bottom:
                    panel = SP_Bottom;
                    break;
                case Dock.Right:
                    panel = SP_Right;
                    break;
            }

            SnapLabel label = new SnapLabel();
            label.Tag = new TAG_Label { window = window };
            label.Text = window.Header.Text;
            label.MouseLeftButtonDown += SnapLabel_MouseLeftButtonDown;
            label.DockedAt = dock;
            label.btnCloseLabel.Click += BtnCloseLabel_Click;
            window.HeaderVisible = dock != Dock.Top;
            panel.Children.Add(label);
            window.Window.LoadSnap();
            SnapLabel_MouseLeftButtonDown(label, default(MouseButtonEventArgs));
        }

        private void BtnCloseLabel_Click(object sender, RoutedEventArgs e)
        {
            SnapLabel label = (((sender as Button).Parent as Grid).Parent as Label).Parent as SnapLabel;

            StackPanel panel;
            Frame frame;
            (label.Tag as TAG_Label).window.Window.UnloadSnap();

            switch (label.DockedAt)
            {
                default:
                    throw new ArgumentException();
                case Dock.Top:
                    panel = SP_Top;
                    frame = F_Top;
                    break;
                case Dock.Left:
                    panel = SP_Left;
                    frame = F_Left;
                    break;
                case Dock.Bottom:
                    panel = SP_Bottom;
                    frame = F_Bottom;
                    break;
                case Dock.Right:
                    panel = SP_Right;
                    frame = F_Right;
                    break;
            }
            if(panel.Children.Count > 1)
            {
                if (label.IsDisplayed)
                {
                    SnapLabel_MouseLeftButtonDown(panel.Children[0], default(MouseButtonEventArgs));
                }
            }
            else
            {
                frame.Content = null;
            }
            panel.Children.Remove(label);
        }

        private void SnapLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SnapLabel l = sender as SnapLabel;
            TAG_Label tag = l.Tag as TAG_Label;
            var wasDisplayed = l.IsDisplayed;
            StackPanel panel;
            Frame frame;
            switch (l.DockedAt)
            {
                default:
                    throw new ArgumentException();
                case Dock.Top:
                    panel = SP_Top;
                    frame = F_Top;
                    break;
                case Dock.Left:
                    panel = SP_Left;
                    frame = F_Left;
                    break;
                case Dock.Bottom:
                    panel = SP_Bottom;
                    frame = F_Bottom;
                    break;
                case Dock.Right:
                    panel = SP_Right;
                    frame = F_Right;
                    break;
            }
            foreach (var it in panel.Children)
            {
                if (it is SnapLabel)
                {
                    (it as SnapLabel).IsDisplayed = false;
                    var tmp = ((it as SnapLabel).Tag as TAG_Label).window;
                    if (tmp.IsDisplayed)
                    {
                        tmp.HideSnap();
                    }
                }
            }
            if (!wasDisplayed)
            {
                tag.window.DisplaySnap(frame);
                l.IsDisplayed = true;
            }
        }
        public List<T> FindSnaps<T>()
        {
            var list = new List<T>();
            foreach (var panel in new [] { SP_Right, SP_Left, SP_Bottom, SP_Top })
            {
                foreach(var it in panel.Children)
                {
                    if(it is SnapLabel)
                    {
                        SnapLabel label = it as SnapLabel;
                        TAG_Label tag = label.Tag as TAG_Label;
                        if(tag.window.WindowContent is T)
                        {
                            list.Add((T)tag.window.WindowContent);
                        }
                    }
                }
            }
            return list;
        }
        public SnapWindow GetFocusedSnapWindow(Dock dock)
        {
            switch(dock)
            {
                default:
                    throw new ArgumentException();
                case Dock.Top:
                    return F_Top.Content as SnapWindow;
                case Dock.Left:
                    return F_Left.Content as SnapWindow;
                case Dock.Bottom:
                    return F_Bottom.Content as SnapWindow;
                case Dock.Right:
                    return F_Right.Content as SnapWindow;
            }
        }
    }
}
