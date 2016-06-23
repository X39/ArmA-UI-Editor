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
using ArmA_UI_Editor.Code.AddInUtil;
using SQF.ClassParser;
using System.Windows.Markup;


namespace ArmA_UI_Editor.UI.Snaps
{
    [ContentProperty("Children")]
    public partial class DialogPropertiesSnap : Page, Code.Interface.ISnapWindow
    {
        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Right; } }

        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly("Children", typeof(UIElementCollection), typeof(Property), new PropertyMetadata());
        public UIElementCollection Children { get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); } private set { SetValue(ChildrenProperty, value); } }
        private EditingSnap _CurrentEditingSnap;
        private EditingSnap CurrentEditingSnap {
            get
            {
                return this._CurrentEditingSnap;
            }
            set
            {
                this._CurrentEditingSnap = value;
            }
        }

        public DialogPropertiesSnap()
        {
            InitializeComponent();
            this.Children = this.PropertyStack.Children;
            _CurrentEditingSnap = null;
        }
        public void UnloadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange -= Docker_OnSnapFocusChange;
        }

        public void LoadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange += Docker_OnSnapFocusChange;
            var EditingSnaps = ArmA_UI_Editor.UI.MainWindow.TryGet().Docker.FindSnaps<EditingSnap>(true);
            if (EditingSnaps.Count > 0)
            {
                this.CurrentEditingSnap = EditingSnaps[0];
            }
        }

        private void Docker_OnSnapFocusChange(object sender, SnapDocker.OnSnapFocusChangeEventArgs e)
        {
            if(e.SnapWindowNew == null)
            {
                if(e.SnapWindowLast != null && e.SnapWindowLast.Window == this.CurrentEditingSnap)
                {
                    this.CurrentEditingSnap = null;
                }
            }
            else if(e.SnapWindowNew.Window is EditingSnap)
            {
                this.CurrentEditingSnap = e.SnapWindowNew.Window as EditingSnap;
            }
        }
    }
}
