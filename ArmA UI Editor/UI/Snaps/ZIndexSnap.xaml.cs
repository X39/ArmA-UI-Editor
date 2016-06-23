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

namespace ArmA_UI_Editor.UI.Snaps
{
    /// <summary>
    /// Interaction logic for PropertyWindow.xaml
    /// </summary>
    public partial class ZIndexSnap : Page, Code.Interface.ISnapWindow
    {
        public Data CurrentData { get; private set; }
        public Code.AddInUtil.Properties CurrentProperties { get; private set; }

        private struct TAG_lbContent
        {
            public List<Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>> Tuple;
            public EditingSnap EditingSnap;
        }
        public int AllowedCount { get { return 1; } }
        public Dock DefaultDock { get { return Dock.Left; } }

        public ZIndexSnap()
        {
            InitializeComponent();
        }

        public void UnloadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange -= Docker_OnSnapFocusChange;
        }

        public void LoadSnap()
        {
            (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.OnSnapFocusChange += Docker_OnSnapFocusChange;
            var EditingSnaps = (ArmA_UI_Editor.UI.MainWindow.TryGet()).Docker.FindSnaps<EditingSnap>(true);
            if(EditingSnaps.Count > 0)
            {
                SubscribeEditingSnap(EditingSnaps[0]);
            }
        }

        private void SubscribeEditingSnap(EditingSnap snap)
        {
            if (lbContent.Tag != null)
            {
                UnSubscribeEditingSnap(((TAG_lbContent)lbContent.Tag).EditingSnap);
            }
            var list = snap.GetUiElements();
            snap.OnUiElementsChanged += EditingSnap_OnUiElementsChanged;
            lbContent.Tag = new TAG_lbContent { EditingSnap = snap, Tuple = list };
            foreach (var it in list)
            {
                lbContent.Items.Add(it);
            }
        }
        private void UnSubscribeEditingSnap(EditingSnap snap)
        {
            if (lbContent.Tag == null)
                return;
            TAG_lbContent tag = (TAG_lbContent)lbContent.Tag;
            if (snap != tag.EditingSnap)
                return;
            tag.EditingSnap.OnUiElementsChanged -= EditingSnap_OnUiElementsChanged;
            lbContent.Items.Clear();
            lbContent.Tag = null;
        }

        private void Docker_OnSnapFocusChange(object sender, SnapDocker.OnSnapFocusChangeEventArgs e)
        {
            if (e.SnapWindowNew != null && e.SnapWindowNew.Window is EditingSnap)
            {
                SubscribeEditingSnap(e.SnapWindowNew.Window as EditingSnap);
            }
            else if(e.SnapWindowLast != null && e.SnapWindowLast.Window is EditingSnap)
            {
                UnSubscribeEditingSnap(e.SnapWindowLast.Window as EditingSnap);
            }
        }

        private void EditingSnap_OnUiElementsChanged(object sender, EventArgs e)
        {
            EditingSnap snap = sender as EditingSnap;
            var list = snap.GetUiElements();
            lbContent.Tag = new TAG_lbContent { EditingSnap = snap, Tuple = list };
            lbContent.Items.Clear();
            foreach (var it in list)
            {
                lbContent.Items.Add(it);
            }
        }

        private void lbContent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.Content, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void lbContent_Drop(object sender, DragEventArgs e)
        {
            var droppedData = e.Data.GetData(typeof(Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>)) as Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>;
            var target = ((ListBoxItem)(sender)).Content as Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>;

            int removedIdx = lbContent.Items.IndexOf(droppedData);
            int targetIdx = lbContent.Items.IndexOf(target);
            if (removedIdx == targetIdx)
                return;

            var snap = ((TAG_lbContent)lbContent.Tag).EditingSnap;
            UnSubscribeEditingSnap(snap);
            snap.SwapUiIndexies(droppedData.Item2.Key, target.Item2.Key);
            SubscribeEditingSnap(snap);
        }
    }
}
