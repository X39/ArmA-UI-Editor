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
            switch(dock)
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
            panel.Children.Add(label);
        }

        private void SnapLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            SnapLabel l = sender as SnapLabel;
            TAG_Label tag = l.Tag as TAG_Label;
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
            foreach(var it in panel.Children)
            {
                if(it is SnapLabel)
                {
                    (it as SnapLabel).IsDisplayed = false;
                    var tmp = ((it as SnapLabel).Tag as TAG_Label).window;
                    if (tmp.IsDisplayed)
                    {
                        tmp.HideSnap();
                    }
                }
            }
            tag.window.DisplaySnap(frame);
            l.IsDisplayed = true;
        }
    }
}
