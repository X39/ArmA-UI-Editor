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
using System.IO;
using ArmA_UI_Editor.Code;
using ArmA_UI_Editor.Code.AddInUtil;
using SQF.ClassParser;
using NLog;
using System.Globalization;

namespace ArmA_UI_Editor.UI.Snaps
{
    /// <summary>
    /// Interaction logic for EditingWindow.xaml
    /// </summary>
    public partial class EditingSnap : Page, Code.Interface.ISnapWindow
    {

        private class UiConverter : Code.Converter.ConfigFieldKeyConverterBase
        {
            private WeakReference<EditingSnap> EditingSnapWeak;
            public UiConverter(EditingSnap creator, string key) : base(key)
            {
                this.EditingSnapWeak = new WeakReference<EditingSnap>(creator);
            }
            public override object DoConvert(ConfigField value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value.IsClass || value.IsString && string.IsNullOrWhiteSpace(value.String))
                    return null;
                var output = value.Value;
                EditingSnap snap;
                if (this.EditingSnapWeak.TryGetTarget(out snap))
                {
                    if (parameter is FieldTypeEnum)
                    {
                        output = value.IsNumber ? value.Number : snap.FromSqfString((FieldTypeEnum)parameter, value.String);
                    }
                }
                return output;
            }

            public override object DoConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var output = value;
                EditingSnap snap;
                if (this.EditingSnapWeak.TryGetTarget(out snap))
                {
                    if (parameter is FieldTypeEnum)
                    {
                        output = value is double ? snap.ToSqfString((FieldTypeEnum)parameter, (double)value) : value;
                    }
                }
                return output;
            }
        }

        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private string GetTraceInfo([System.Runtime.CompilerServices.CallerMemberName] string caller = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
        {
            return string.Format("fnc: {0} at: {1}", caller, line);
        }
        public class TAG_CanvasChildElement
        {
            public string Key { get; set; }
            public Code.AddInUtil.UIElement file { get; set; }
            public EditingSnap Owner { get; set; }
        }

        #region Events
        public event EventHandler OnUiElementsChanged;
        public class OnSelectedFocusChangedEventArgs : EventArgs
        {
            public TAG_CanvasChildElement[] Tags;
            
            public OnSelectedFocusChangedEventArgs(params TAG_CanvasChildElement[] Tags)
            {
                this.Tags = Tags;
            }
        }
        public event EventHandler<OnSelectedFocusChangedEventArgs> OnSelectedFocusChanged;
        #endregion
        public SQF.ClassParser.ConfigField Config;
        public static int Counter = 0;
        public SQF.ClassParser.ConfigField[] FileConfigs { get { return this.Config[0, this.Config.Count]; } }
        public SQF.ClassParser.ConfigField LastFileConfig { get { return this.Config[this.Config.Count - 1]; } }

        public static readonly DependencyProperty SnapDisabledProperty = DependencyProperty.Register("SnapDisabled", typeof(bool), typeof(EditingSnap));
        public bool SnapEnabled { get { return (bool)GetValue(SnapDisabledProperty); } set { SetValue(SnapDisabledProperty, value); } }
        public static readonly DependencyProperty BackgroundEnabledProperty = DependencyProperty.Register("BackgroundEnabled", typeof(bool), typeof(EditingSnap));
        public bool BackgroundEnabled { get { return (bool)GetValue(BackgroundEnabledProperty); } set { SetValue(BackgroundEnabledProperty, value); } }

        public static readonly DependencyProperty SnapGridProperty = DependencyProperty.Register("SnapGrid", typeof(Rect), typeof(EditingSnap));
        public int SnapGrid { get { return (int)((Rect)GetValue(SnapGridProperty)).Width; } set { SetValue(SnapGridProperty, new Rect(0, 0, value, value)); } }

        public static readonly DependencyProperty ViewScaleProperty = DependencyProperty.Register("ViewScale", typeof(double), typeof(EditingSnap));
        public double ViewScale
        {
            get
            {
                return (double)GetValue(ViewScaleProperty);
            }
            set
            {
                SetValue(ViewScaleProperty, value);
                var overlay = FindSelectionOverlay();
                if (overlay != null)
                {
                    overlay.PullThickness = (int)(SelectionOverlay.DefaultThickness / value);
                }
            }
        }

        public string FilePath { get; set; }
        public int AllowedCount { get { return int.MaxValue; } }

        #region ISnapWindow
        public Dock DefaultDock { get { return Dock.Top; } }
        public bool HasUnsavedChanges { get; private set; }
        public bool AllowConfigPatching { get; private set; }

        public void UnloadSnap()
        {
            if (this.HasUnsavedChanges)
            {
                if (MessageBox.Show("Do you want to save?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
            }
            this.Config.Parent.RemoveKey(this.Config.Name);
        }
        public void LoadSnap()
        {
            this.Config = AddInManager.Instance.MainFile.AddKey(string.Format("EditingSnap_{0}_WorkingConfig", Counter++));
            this.Config.ToClass();
            using (var stream = this.Textbox.Text.AsMemoryStream())
            {
                SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(stream));
                p.Patch(this.Config, true);
            }
            this.RegenerateDisplay();
        }
        #endregion

        public EditingSnap()
        {
            InitializeComponent();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("class NewArmAUI");
            sb.AppendLine("{");
            sb.AppendLine("\tidd = -1;");
            sb.AppendLine("\tclass controls");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tclass MyFirstRscText : RscText");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tw = \"(120 / 1920) * SafeZoneW\";");
            sb.AppendLine("\t\t\th = \"(30 / 1080) * SafeZoneH\";");
            sb.AppendLine("\t\t\ttext = \"My UI Starts here <3\";");
            sb.AppendLine("\t\t\tcolorBackground[] = {0.1, 0.1, 0.1, 0.5};");
            sb.AppendLine("\t\t};");
            sb.AppendLine("\t};");
            sb.AppendLine("};");

            this.Textbox.Text = sb.ToString();
            this.SnapEnabled = true;
            this.BackgroundEnabled = false;
            this.SnapGrid = 15;
            this.ViewScale = 1;
            this.FilePath = string.Empty;
            this.HasUnsavedChanges = true;
            this.AllowConfigPatching = false;
        }
        public EditingSnap(string FilePath)
        {
            InitializeComponent();
            this.Config = AddInManager.Instance.MainFile.AddKey(string.Format("EditingSnap_{0}_WorkingConfig", Counter));
            using (var reader = new StreamReader(FilePath))
            {
                this.Textbox.Text = reader.ReadToEnd();
            }

            this.SnapEnabled = true;
            this.BackgroundEnabled = false;
            this.SnapGrid = 15;
            this.ViewScale = 1;
            this.FilePath = FilePath;
            this.HasUnsavedChanges = false;
            this.AllowConfigPatching = false;
        }
        private void ReReadConfigField()
        {
            throw new NotImplementedException();
        }

        public void SaveFile()
        {
            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = this.LastFileConfig.Name;
                dlg.DefaultExt = ".cpp";
                dlg.Filter = "ArmA Class (.cpp)|*.cpp|ArmA Class (.hpp)|*.hpp";
                dlg.CheckPathExists = true;
                var res = dlg.ShowDialog();
                if (!res.HasValue || !res.Value)
                {
                    return;
                }
                this.FilePath = dlg.FileName;

            }
            using (var writer = new StreamWriter(this.FilePath))
            {
                writer.Write(this.Textbox.Text);
                writer.Flush();
            }
            HasUnsavedChanges = false;
        }
        private bool RemoveSelectedElements()
        {
            foreach (var it in this.DisplayCanvas.Children)
            {
                if (it is SelectionOverlay)
                {
                    var overlay = it as SelectionOverlay;
                    foreach (var el in overlay.ToggledElements)
                    {
                        if (el is FrameworkElement)
                        {
                            var fElement = el as FrameworkElement;
                            RemoveElement(fElement);
                        }
                    }
                    DisplayCanvas.Children.Remove(overlay);
                    return true;
                }
            }
            return false;
        }
        private void RemoveElement(FrameworkElement el)
        {
            var lastFileConfig = this.LastFileConfig;
            var controlsConfig = lastFileConfig["controls"];
            var tagData = el.Tag as TAG_CanvasChildElement;
            controlsConfig.RemoveKey(tagData.Key);
            DisplayCanvas.Children.Remove(el);
        }

        public void SelectElements(bool mouseDownOnCreateOverlay, bool focusPropertyPane, params FrameworkElement[] elements)
        {
            if(elements.Length == 0)
            {
                var overlay = this.FindSelectionOverlay();
                if(overlay != null)
                {
                    this.DisplayCanvas.Children.Remove(overlay);
                }
            }
            else
            {
                var overlay = this.CreateOrGetSelectionOverlay(mouseDownOnCreateOverlay);
                if (!overlay.ToggledElements.SequenceEqual(elements))
                {
                    overlay.ToggledElements.Clear();
                    overlay.ToggledElements.AddRange(elements);
                    overlay.UpdateMetrics();
                }
            }
            if (this.OnSelectedFocusChanged != null)
            {
                List<TAG_CanvasChildElement> tagList = new List<TAG_CanvasChildElement>();
                foreach(var it in elements)
                {
                    if(it.Tag is TAG_CanvasChildElement)
                    {
                        tagList.Add(it.Tag as TAG_CanvasChildElement);
                    }
                }
                this.OnSelectedFocusChanged(this, new OnSelectedFocusChangedEventArgs(tagList.ToArray()));
            }
            if (focusPropertyPane)
            {
                if (MainWindow.TryGet().HasSnapInstance<PropertySnap>())
                {
                    MainWindow.TryGet().AddToDockerOrFocus<PropertySnap>();
                }
            }
        }

        #region SelectionOverlay Event Handler
        private void SelectionOverlay_OnStartMove(object sender, MouseEventArgs e)
        {
            FindSelectionOverlay().Tag = e.GetPosition(DisplayCanvas);
        }
        private void SelectionOverlay_OnElementResize(object sender, SelectionOverlay.ResizeEventArgs e)
        {
            bool posChangeNeeded = new SelectionOverlay.ResizeEventArgs.Direction[] {
                SelectionOverlay.ResizeEventArgs.Direction.TopRight,
                SelectionOverlay.ResizeEventArgs.Direction.Top,
                SelectionOverlay.ResizeEventArgs.Direction.TopLeft,
                SelectionOverlay.ResizeEventArgs.Direction.Left,
                SelectionOverlay.ResizeEventArgs.Direction.BotLeft
            }.Contains(e.Dir);
            var metrics = e.Element.GetCanvasMetrics();
            if (posChangeNeeded)
            {
                metrics.X -= e.DeltaX;
                metrics.Y -= e.DeltaY;
            }
            if (metrics.Width + e.DeltaX >= 0)
                metrics.Width += e.DeltaX;

            if (metrics.Height + e.DeltaY >= 0)
                metrics.Height += e.DeltaY;
            if (e.Element is FrameworkElement)
            {
                var fElement = e.Element as FrameworkElement;
                var data = fElement.Tag as TAG_CanvasChildElement;
                if (data == null)
                    return;

                if (posChangeNeeded)
                {
                    AddInManager.Instance.MainFile.SetKey(string.Join("/", data.Key, "x"), ToSqfString(FieldTypeEnum.XField, metrics.X));
                    AddInManager.Instance.MainFile.SetKey(string.Join("/", data.Key, "y"), ToSqfString(FieldTypeEnum.YField, metrics.Y));
                }
                AddInManager.Instance.MainFile.SetKey(string.Join("/", data.Key, "w"), ToSqfString(FieldTypeEnum.WField, metrics.Width));
                AddInManager.Instance.MainFile.SetKey(string.Join("/", data.Key, "h"), ToSqfString(FieldTypeEnum.HField, metrics.Height));
            }
        }
        private void SelectionOverlay_OnElementMove(object sender, SelectionOverlay.MoveEventArgs e)
        {
            var metrics = e.Element.GetCanvasMetrics();
            metrics.X += e.DeltaX;
            metrics.Y += e.DeltaY;
            if (e.Element is FrameworkElement)
            {
                var fElement = e.Element as FrameworkElement;
                var data = fElement.Tag as TAG_CanvasChildElement;
                if (data == null)
                    return;
                
                AddInManager.Instance.MainFile.SetKey(string.Join("/", data.Key, "x"), ToSqfString(FieldTypeEnum.XField, metrics.X));
                AddInManager.Instance.MainFile.SetKey(string.Join("/", data.Key, "y"), ToSqfString(FieldTypeEnum.YField, metrics.Y));
            }
        }
        private void SelectionOverlay_OnOperationFinalized(object sender, FrameworkElement[] e)
        {
            SelectElements(false, false, e);
        }
        #endregion
        #region XAML Event Handler
        private void SizesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            switch ((cb.SelectedValue as ComboBoxItem).Content as string)
            {
                case "20%":
                    this.ViewScale = 0.2;
                    break;
                case "40%":
                    this.ViewScale = 0.4;
                    break;
                case "60%":
                    this.ViewScale = 0.6;
                    break;
                case "80%":
                    this.ViewScale = 0.8;
                    break;
                case "100%":
                    this.ViewScale = 1;
                    break;
                case "120%":
                    this.ViewScale = 1.2;
                    break;
                case "140%":
                    this.ViewScale = 1.4;
                    break;
                case "160%":
                    this.ViewScale = 1.6;
                    break;
                case "180%":
                    this.ViewScale = 1.8;
                    break;
                case "200%":
                    this.ViewScale = 2;
                    break;
            }
        }
        private void GridScaleBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.All((c) => char.IsDigit(c)))
            {
                e.Handled = true;
            }
        }
        private void GridScaleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length > 0)
            {
                this.SnapGrid = int.Parse((sender as TextBox).Text);
            }
        }
        private void DisplayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var arr = this.DisplayCanvas.Children;
            System.Windows.FrameworkElement thisElement = null;
            SelectionOverlay overlay = null;
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                var it = arr[i] as FrameworkElement;
                if (it is SelectionOverlay)
                {
                    overlay = it as SelectionOverlay;
                    continue;
                }
                var itMetrics = it.GetCanvasMetrics();
                var mousePosRelToIt = e.GetPosition(it);
                if (mousePosRelToIt.X <= itMetrics.Width && mousePosRelToIt.X >= 0 && mousePosRelToIt.Y <= itMetrics.Height && mousePosRelToIt.Y >= 0)
                {
                    thisElement = it;
                    break;
                }
            }
            if (thisElement == null)
            {
                if (overlay != null)
                {
                    var overlayMetrics = overlay.GetCanvasMetrics();
                    var mousePosRelToOverlay = e.GetPosition(overlay);
                    if (mousePosRelToOverlay.X > overlayMetrics.Width || mousePosRelToOverlay.X < 0 || mousePosRelToOverlay.Y > overlayMetrics.Height || mousePosRelToOverlay.Y < 0)
                    {
                        SelectElements(false, false);
                    }
                }
                return;
            }



            if (overlay == null)
            {
                SelectElements(true, true, thisElement);
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                List<FrameworkElement> fElements = new List<FrameworkElement>(overlay.ToggledElements);
                if (fElements.Contains(thisElement))
                {
                    fElements.Remove(thisElement);
                }
                else
                {
                    fElements.Add(thisElement);
                }
                SelectElements(true, true, fElements.ToArray());
            }
            else
            {
                var overlayMetrics = overlay.GetCanvasMetrics();
                var mousePosRelToOverlay = e.GetPosition(overlay);
                if (mousePosRelToOverlay.X > overlayMetrics.Width || mousePosRelToOverlay.X < 0 || mousePosRelToOverlay.Y > overlayMetrics.Height || mousePosRelToOverlay.Y < 0)
                {
                    SelectElements(true, true, thisElement);
                }
            }
            e.Handled = true;
        }
        private void DisplayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (var it in this.DisplayCanvas.Children)
            {
                if (it is SelectionOverlay)
                {
                    (it as SelectionOverlay).ReleaseMove();
                    break;
                }
            }
        }
        private void DisplayCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var arr = this.DisplayCanvas.Children;
            FrameworkElement thisElement = null;
            SelectionOverlay overlay = null;
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                var it = arr[i] as FrameworkElement;
                if (it is SelectionOverlay)
                {
                    overlay = it as SelectionOverlay;
                    continue;
                }
                var itMetrics = it.GetCanvasMetrics();
                var mousePosRelToIt = e.GetPosition(it);
                if (mousePosRelToIt.X <= itMetrics.Width && mousePosRelToIt.X >= 0 && mousePosRelToIt.Y <= itMetrics.Height && mousePosRelToIt.Y >= 0)
                {
                    thisElement = it;
                    break;
                }
            }

            if (thisElement == null && overlay == null)
            {
                ContextMenu cm = this.FindResource("ContextMenu_Canvas") as ContextMenu;
                cm.PlacementTarget = this.DisplayCanvas;
                cm.IsOpen = true;
            }
            else if (overlay != null)
            {
                var overlayMetrics = overlay.GetCanvasMetrics();
                var mousePosRelToOverlay = e.GetPosition(overlay);
                if (mousePosRelToOverlay.X > overlayMetrics.Width || mousePosRelToOverlay.X < 0 || mousePosRelToOverlay.Y > overlayMetrics.Height || mousePosRelToOverlay.Y < 0)
                {
                    ContextMenu cm = this.FindResource("ContextMenu_Canvas") as ContextMenu;
                    cm.PlacementTarget = this.DisplayCanvas;
                    cm.IsOpen = true;
                }
                else
                {
                    if (overlay.ToggledElements.Count == 1)
                    {
                        ContextMenu cm = this.FindResource("ContextMenu_ChildElement") as ContextMenu;
                        cm.Tag = thisElement;
                        cm.PlacementTarget = this.DisplayCanvas;
                        cm.IsOpen = true;
                    }
                    else
                    {
                        ContextMenu cm = this.FindResource("ContextMenu_ChildElements") as ContextMenu;
                        cm.Tag = overlay.ToggledElements;
                        cm.PlacementTarget = this.DisplayCanvas;
                        cm.IsOpen = true;
                    }
                }
            }
            else
            {
                ContextMenu cm = this.FindResource("ContextMenu_ChildElement") as ContextMenu;
                cm.Tag = thisElement;
                cm.PlacementTarget = this.DisplayCanvas;
                cm.IsOpen = true;
            }
        }
        private void DisplayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var overlay = FindSelectionOverlay();
            if (overlay != null && overlay.MoveState != SelectionOverlay.MoveStateEnum.NONE)
            {
                var pos = e.GetPosition(DisplayCanvas);
                if(overlay.Tag == null)
                {
                    overlay.Tag = pos;
                }
                var oldPos = (Point)overlay.Tag;
                var deltaX = pos.X - oldPos.X;
                var deltaY = pos.Y - oldPos.Y;
                if (SnapEnabled)
                {
                    deltaX -= deltaX % SnapGrid;
                    deltaY -= deltaY % SnapGrid;
                }
                if (deltaX != 0 || deltaY != 0)
                {
                    if (SnapEnabled)
                    {
                        pos.X -= (pos.X - oldPos.X) % SnapGrid;
                        pos.Y -= (pos.Y - oldPos.Y) % SnapGrid;
                    }
                    overlay.Tag = pos;
                    overlay.DoMove(deltaX, deltaY);
                }
            }
        }
        private void DisplayCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("UiElementsListBoxData"))
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void DisplayCanvas_Drop(object sender, DragEventArgs e)
        {
            Logger.Trace(string.Format("{0} args: {1}", this.GetTraceInfo(), string.Join(", ", sender, e)));
            if (!e.Data.GetDataPresent("UiElementsListBoxData"))
                return;
            var dragDropData = (e.Data.GetData("UiElementsListBoxData") as Code.UI.DragDrop.UiElementsListBoxData);
            var controlsField = this.LastFileConfig.GetKey("controls", ConfigField.KeyMode.CreateNew);

            string name = string.Concat("My" + dragDropData.ElementData.ConfigKey);
            int count = 1;
            while (controlsField.Contains(name))
            {
                count++;
                name = string.Concat("My" + dragDropData.ElementData.ConfigKey, count);
            }
            var field = controlsField.AddKey(name, dragDropData.ElementData.ConfigKey);
            var mousePos = e.GetPosition(this.DisplayCanvas);
            field.SetKey("x", this.ToSqfString(FieldTypeEnum.XField, mousePos.X));
            field.SetKey("y", this.ToSqfString(FieldTypeEnum.YField, mousePos.Y));

            var widthField = field.GetKey("w", ConfigField.KeyMode.CheckParentsNull);
            if (widthField != null)
            {
                double d = widthField.IsNumber ? widthField.Number : this.FromSqfString(FieldTypeEnum.HField, widthField.String);
                field.SetKey("w", this.ToSqfString(FieldTypeEnum.WField, d));
            }
            var heightField = field.GetKey("h", ConfigField.KeyMode.CheckParentsNull);
            if (heightField != null)
            {
                double d = heightField.IsNumber ? heightField.Number : this.FromSqfString(FieldTypeEnum.HField, heightField.String);
                field.SetKey("h", this.ToSqfString(FieldTypeEnum.HField, d));
            }
            using (var stream = this.Textbox.Text.AsMemoryStream())
            {
                SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(stream));
                var searchKey = controlsField.Key;
                searchKey = searchKey.Remove(0, searchKey.IndexOf(this.LastFileConfig.Name) - 1);
                var index = p.GetValueRange(searchKey);

                this.Textbox.Text = this.Textbox.Text.Insert(index.Item2, string.Concat("\r\n", field.ToPrintString(2)));
            }
            this.RegenerateDisplay();
            this.ThisScrollViewer.Focus();
        }
        private void ScrollViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (this.RemoveSelectedElements())
                    e.Handled = true;
            }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.SaveFile();
            }
        }


        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.HasUnsavedChanges = true;
            this.AllowConfigPatching = true;
            foreach (var change in e.Changes)
            {
                if (change.AddedLength == 1)
                {
                    string changedString = Textbox.Text.Substring(change.Offset, change.AddedLength);
                    if (changedString == "\t")
                    {
                        //ToDo: Insert required tabs automatically
                    }
                }
                else if (change.AddedLength == 2)
                {
                    if (Textbox.Text.Substring(change.Offset, change.AddedLength) == "\r\n")
                    {
                        string before = Textbox.Text.Substring(0, change.Offset);
                        int lastNewLineIndex = before.LastIndexOf('\n');
                        if (lastNewLineIndex == -1)
                            continue;
                        int tabCount = before.LastIndexOf('\t') - lastNewLineIndex;
                        if (tabCount < 0)
                            continue;
                        Textbox.Text = Textbox.Text.Insert(change.Offset + change.AddedLength, new string('\t', tabCount));
                        Textbox.SelectionStart = change.Offset + change.AddedLength + tabCount;
                    }
                }
                if (change.RemovedLength == 1)
                {
                    string changedString = Textbox.Text.Substring(change.Offset - change.RemovedLength, change.RemovedLength);
                    if (changedString == "\t")
                    {
                        //ToDo: Remove whole tab group if only tabs are remaining in this row
                    }
                }
            }
        }
        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!this.AllowConfigPatching)
                return;
            this.AllowConfigPatching = false;
            using (var stream = this.Textbox.Text.AsMemoryStream())
            {
                SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(stream));
                p.Patch(this.Config, true);
            }
            this.RegenerateDisplay();
        }
        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.SaveFile();
                e.Handled = true;
            }
        }
        private void TabControlMainView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Config == null)
                return;
            //if (this.TabControlMainView.SelectedIndex == 1)
            //{
            //    this.RegenerateDisplay();
            //}
        }
        private void TabControlMainView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.SaveFile();
                e.Handled = true;
            }
        }
        #endregion

        public bool RegenerateDisplay()
        {
            Logger.Trace(string.Format("{0} args: -", this.GetTraceInfo()));
            var mainWindow = ArmA_UI_Editor.UI.MainWindow.TryGet();
            try
            {
                int initialCount = 0;
                foreach (var it in this.DisplayCanvas.Children)
                {
                    if (it is SelectionOverlay)
                        continue;
                    initialCount++;
                }
                this.DisplayCanvas.Children.Clear();
                var controlsField = this.LastFileConfig["controls"];
                if (!controlsField.IsClass)
                    throw new ArgumentException("controls has to be a class (array syntax not yet supported)");
                bool restoredSelection = false;
                int index = 0;
                foreach (var value in controlsField)
                {
                    if (!(value is ConfigField))
                    {
                        continue;
                    }
                    var curField = value as ConfigField;
                    if(string.IsNullOrWhiteSpace(curField.ConfigParentName))
                    {
                        Logger.Warn(string.Format(App.Current.Resources["STR_Snaps_EditingSnap_Logger_Warning_IgnoringElementAsNoBaseClass"] as String), curField.Key);
                        continue;
                    }
                    //ToDo: Check ALL base classes if they implement an AddIn (--> ConfigField function/field)
                    var addInUiElement = AddInManager.Instance.GetElement(curField.ConfigParentName);
                    if (addInUiElement == null)
                    {
                        Logger.Warn(string.Format(App.Current.Resources["STR_Snaps_EditingSnap_Logger_Warning_IgnoringElementAsNotDefinedInAddins"] as String), curField.Key);
                        continue;
                    }
                    Code.Markup.BindConfig.AddInPath = addInUiElement.Parent.ThisPath;
                    using (FileStream stream = System.IO.File.OpenRead(addInUiElement.__XamlPath))
                    {
                        Code.Markup.BindConfig.CurrentClassPath = curField.Key;

                        var el = (FrameworkElement)System.Windows.Markup.XamlReader.Load(stream);
                        this.DisplayCanvas.Children.Add(el);
                        Binding binding;

                        binding = new Binding("Value");
                        binding.Source = AddInManager.Instance.MainFile;
                        binding.Converter = new UiConverter(this, string.Concat(curField.Key, "/x"));
                        binding.ConverterParameter = FieldTypeEnum.XField;
                        el.SetBinding(Canvas.LeftProperty, binding);

                        binding = new Binding("Value");
                        binding.Source = AddInManager.Instance.MainFile;
                        binding.Converter = new UiConverter(this, string.Concat(curField.Key, "/w"));
                        binding.ConverterParameter = FieldTypeEnum.WField;
                        el.SetBinding(Canvas.WidthProperty, binding);

                        binding = new Binding("Value");
                        binding.Source = AddInManager.Instance.MainFile;
                        binding.Converter = new UiConverter(this, string.Concat(curField.Key, "/y"));
                        binding.ConverterParameter = FieldTypeEnum.YField;
                        el.SetBinding(Canvas.TopProperty, binding);

                        binding = new Binding("Value");
                        binding.Source = AddInManager.Instance.MainFile;
                        binding.Converter = new UiConverter(this, string.Concat(curField.Key, "/h"));
                        binding.ConverterParameter = FieldTypeEnum.HField;
                        el.SetBinding(Canvas.HeightProperty, binding);

                        Canvas.SetZIndex(el, index);

                        el.Tag = new TAG_CanvasChildElement { file = addInUiElement, Key = curField.Key, Owner = this };
                        var snaps = MainWindow.TryGet().Docker.FindSnaps<PropertySnap>();
                        //ToDo: Restore selection
                    }
                    if (!restoredSelection)
                    {
                        SelectElements(false, false);
                    }
                    index++;
                }
                mainWindow.SetStatusbarText("", false);
                foreach (var it in this.DisplayCanvas.Children)
                {
                    initialCount--;
                }
                if (initialCount != 0 && this.OnUiElementsChanged != null)
                    OnUiElementsChanged(this, new EventArgs());
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                mainWindow.SetStatusbarText(App.Current.Resources["STR_CODE_EditingWindow_ConfigParsingError"] as String, true);
                this.DisplayCanvas.Children.Clear();
                Frame frame = new Frame();
                frame.Content = new ParseError();
                this.DisplayCanvas.Children.Add(frame);
            }
            return false;
        }

        private SelectionOverlay CreateOrGetSelectionOverlay(bool mouseDownOnCreate = true)
        {
            Logger.Trace(string.Format("{0} args: {1}", this.GetTraceInfo(), string.Join(", ", mouseDownOnCreate)));
            SelectionOverlay el = FindSelectionOverlay();
            if (el == null)
            {
                el = new SelectionOverlay(mouseDownOnCreate);
                el.OnElementMove += SelectionOverlay_OnElementMove;
                el.OnElementResize += SelectionOverlay_OnElementResize;
                el.OnStartMove += SelectionOverlay_OnStartMove;
                //el.OnStopMove += SelectionOverlay_OnStopMove;
                el.OnOperationFinalized += SelectionOverlay_OnOperationFinalized;
                Canvas.SetZIndex(el, 10000);
                this.DisplayCanvas.Children.Add(el);
                el.PullThickness = (int)(SelectionOverlay.DefaultThickness / this.ViewScale);
            }
            return el;
        }
        private SelectionOverlay FindSelectionOverlay()
        {
            SelectionOverlay el = null;
            foreach (var it in this.DisplayCanvas.Children)
            {
                if (it is SelectionOverlay)
                {
                    el = it as SelectionOverlay;
                    break;
                }
            }
            return el;
        }

        #region ContextMenu Click EventHandlers
        #region ChildElement ContextMenu
        private void ContextMenu_ChildElement_FitToGrid_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is FrameworkElement)
            {
                var el = cm.Tag as FrameworkElement;
                if (el.Tag is TAG_CanvasChildElement)
                {
                    var tag = el.Tag as TAG_CanvasChildElement;
                    var snapGrid = tag.Owner.SnapGrid;
                    var metricts = Code.Utility.GetCanvasMetrics(el);
                    double tmp;
                    tmp = metricts.Right % snapGrid;
                    var deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                    tmp = metricts.Bottom % snapGrid;
                    var deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                    if (deltaX != 0 || deltaY != 0)
                        SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.BotRight, deltaX, deltaY, el));
                }
            }
        }
        private void ContextMenu_ChildElement_SnapToGrid_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is FrameworkElement)
            {
                var el = cm.Tag as FrameworkElement;
                if (el.Tag is TAG_CanvasChildElement)
                {
                    var tag = el.Tag as TAG_CanvasChildElement;
                    var snapGrid = tag.Owner.SnapGrid;
                    var metricts = Code.Utility.GetCanvasMetrics(el);
                    double tmp;
                    tmp = metricts.X % snapGrid;
                    var deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                    tmp = metricts.Y % snapGrid;
                    var deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                    if (deltaX != 0 || deltaY != 0)
                        SelectionOverlay_OnElementMove(cm, new SelectionOverlay.MoveEventArgs(deltaX, deltaY, el));
                }
            }
        }
        private void ContextMenu_ChildElement_SnapFitToGrid_Click(object sender, RoutedEventArgs e)
        {
            this.ContextMenu_ChildElement_SnapToGrid_Click(sender, e);
            this.ContextMenu_ChildElement_FitToGrid_Click(sender, e);
            //var mi = sender as MenuItem;
            //var cm = mi.Parent as ContextMenu;
            //if (cm.Tag is FrameworkElement)
            //{
            //    var el = cm.Tag as FrameworkElement;
            //    if (el.Tag is TAG_CanvasChildElement)
            //    {
            //        var tag = el.Tag as TAG_CanvasChildElement;
            //        var snapGrid = tag.Owner.SnapGrid;
            //        var metricts = Code.Utility.GetCanvasMetrics(el);
            //        double tmp;
            //        tmp = metricts.Right % snapGrid;
            //        var deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
            //        tmp = metricts.Bottom % snapGrid;
            //        var deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
            //        if (deltaX != 0 || deltaY != 0)
            //            SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.BotRight, deltaX, deltaY, el));
            //
            //        metricts = Code.Utility.GetCanvasMetrics(el);
            //        tmp = metricts.X % snapGrid;
            //        deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
            //        tmp = metricts.Y % snapGrid;
            //        deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
            //        if (deltaX != 0 || deltaY != 0)
            //            SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.TopLeft, -deltaX, -deltaY, el));
            //    }
            //}
        }
        private void ContextMenu_ChildElement_Properties_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is FrameworkElement)
            {
                var el = cm.Tag as FrameworkElement;
                if (el.Tag is TAG_CanvasChildElement)
                {
                    SelectElements(false, true, el);
                }
            }
        }
        private void ContextMenu_ChildElement_Delete_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is FrameworkElement)
            {
                var el = cm.Tag as FrameworkElement;
                RemoveElement(el);
            }
        }
        private void ContextMenu_ChildElements_Delete_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedElements();
        }
        #endregion
        #region ChildElements ContextMenu
        private void ContextMenu_ChildElements_FitToGrid_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is List<FrameworkElement>)
            {
                foreach (var el in cm.Tag as List<FrameworkElement>)
                {
                    if (el.Tag is TAG_CanvasChildElement)
                    {
                        var tag = el.Tag as TAG_CanvasChildElement;
                        var snapGrid = tag.Owner.SnapGrid;
                        var metricts = Code.Utility.GetCanvasMetrics(el);
                        double tmp;
                        tmp = metricts.Right % snapGrid;
                        var deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        tmp = metricts.Bottom % snapGrid;
                        var deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        if (deltaX != 0 || deltaY != 0)
                            SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.BotRight, deltaX, deltaY, el));
                    }
                }
                foreach (var it in this.DisplayCanvas.Children)
                {
                    if (it is SelectionOverlay)
                    {
                        (it as SelectionOverlay).UpdateMetrics();
                        break;
                    }
                }
            }
        }
        private void ContextMenu_ChildElements_SnapToGrid_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is List<FrameworkElement>)
            {
                foreach (var el in cm.Tag as List<FrameworkElement>)
                {
                    if (el.Tag is TAG_CanvasChildElement)
                    {
                        var tag = el.Tag as TAG_CanvasChildElement;
                        var snapGrid = tag.Owner.SnapGrid;
                        var metricts = Code.Utility.GetCanvasMetrics(el);
                        double tmp;
                        tmp = metricts.X % snapGrid;
                        var deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        tmp = metricts.Y % snapGrid;
                        var deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        if (deltaX != 0 || deltaY != 0)
                            SelectionOverlay_OnElementMove(cm, new SelectionOverlay.MoveEventArgs(deltaX, deltaY, el));
                    }
                }
                foreach (var it in this.DisplayCanvas.Children)
                {
                    if (it is SelectionOverlay)
                    {
                        (it as SelectionOverlay).UpdateMetrics();
                        break;
                    }
                }
            }
        }
        private void ContextMenu_ChildElements_SnapFitToGrid_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            if (cm.Tag is List<FrameworkElement>)
            {
                foreach (var el in cm.Tag as List<FrameworkElement>)
                {
                    if (el.Tag is TAG_CanvasChildElement)
                    {
                        var tag = el.Tag as TAG_CanvasChildElement;
                        var snapGrid = tag.Owner.SnapGrid;
                        var metricts = Code.Utility.GetCanvasMetrics(el);
                        double tmp;
                        tmp = metricts.Right % snapGrid;
                        var deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        tmp = metricts.Bottom % snapGrid;
                        var deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        if (deltaX != 0 || deltaY != 0)
                            SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.BotRight, deltaX, deltaY, el));

                        metricts = Code.Utility.GetCanvasMetrics(el);
                        tmp = metricts.X % snapGrid;
                        deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        tmp = metricts.Y % snapGrid;
                        deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                        if (deltaX != 0 || deltaY != 0)
                            SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.TopLeft, -deltaX, -deltaY, el));
                    }
                }
                foreach (var it in this.DisplayCanvas.Children)
                {
                    if (it is SelectionOverlay)
                    {
                        (it as SelectionOverlay).UpdateMetrics();
                        break;
                    }
                }
            }
        }
        #endregion
        #endregion

        public enum FieldTypeEnum
        {
            XField,
            YField,
            WField,
            HField
        }
        public string ToSqfString(FieldTypeEnum fieldType, double data)
        {
            Logger.Trace(string.Format("{0} args: {1}", this.GetTraceInfo(), string.Join(", ", fieldType, data)));
            double max;
            StringBuilder builder = new StringBuilder();
            switch (fieldType)
            {
                case FieldTypeEnum.XField:
                    max = 1920;
                    builder.Append("SafeZoneX + ");
                    builder.Append('(');
                    builder.Append(data.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" / ");
                    builder.Append(max.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(')');
                    builder.Append(" * SafeZoneW");
                    break;
                case FieldTypeEnum.YField:
                    max = 1080;
                    builder.Append("SafeZoneY + ");
                    builder.Append('(');
                    builder.Append(data.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" / ");
                    builder.Append(max.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(')');
                    builder.Append(" * SafeZoneH");
                    break;
                case FieldTypeEnum.WField:
                    max = 1920;
                    builder.Append('(');
                    builder.Append(data.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" / ");
                    builder.Append(max.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(')');
                    builder.Append(" * SafeZoneW");
                    break;
                case FieldTypeEnum.HField:
                    max = 1080;
                    builder.Append('(');
                    builder.Append(data.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" / ");
                    builder.Append(max.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(')');
                    builder.Append(" * SafeZoneH");
                    break;
                default:
                    throw new Exception();
            }
            return builder.ToString();
        }
        public double FromSqfString(FieldTypeEnum fieldType, string data)
        {
            Logger.Trace(string.Format("{0} args: {1}", this.GetTraceInfo(), string.Join(", ", fieldType, data)));
            data = data.ToUpper();
            double max;
            switch (fieldType)
            {
                case FieldTypeEnum.XField:
                case FieldTypeEnum.WField:
                    data = data.Replace("SAFEZONEX", "0");
                    data = data.Replace("SAFEZONEW", "1");
                    max = 1920;
                    break;
                case FieldTypeEnum.YField:
                case FieldTypeEnum.HField:
                    data = data.Replace("SAFEZONEY", "0");
                    data = data.Replace("SAFEZONEH", "1");
                    max = 1080;
                    break;
                default:
                    throw new Exception();
            }

            var dt = new System.Data.DataTable();
            return (double.Parse(dt.Compute(data, "").ToString())) * max;
        }
        public void UpdateConfigKey(string key)
        {
            using (var stream = this.Textbox.Text.AsMemoryStream())
            {
                SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(stream));
                key = key.Remove(0, key.IndexOf(this.LastFileConfig.Name) - 1);
                var index = p.GetValueRange(key);
                if (index == null)
                {
                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(stream));
                    index = p.GetValueRange(key.Remove(key.LastIndexOf('/')));
                    var field = this.Config.GetKey(key, ConfigField.KeyMode.ThrowOnNotFound);
                    this.Textbox.Text = this.Textbox.Text.Insert(index.Item1, string.Concat(field.ToPrintString(), "\r\n", new string('\t', key.Count((c) => c == '/') - 1)));
                }
                else
                {
                    var field = this.Config.GetKey(key, ConfigField.KeyMode.ThrowOnNotFound);
                    this.Textbox.Text = this.Textbox.Text.Remove(index.Item1, index.Item2 - index.Item1).Insert(index.Item1, field.ToValueString());
                }
            }
        }

        public List<Tuple<Code.AddInUtil.UIElement, string>> GetUiElements()
        {
            Logger.Trace(string.Format("{0} args: -", this.GetTraceInfo()));
            var list = new List<Tuple<Code.AddInUtil.UIElement, string>>();
            var controls = this.LastFileConfig["controls"];
            foreach (var value in controls)
            {
                if (!(value is ConfigField))
                {
                    continue;
                }
                var curField = value as ConfigField;
                //ToDo: Check ALL base classes if they implement an AddIn (--> ConfigField function/field)
                var curFieldParent = AddInManager.Instance.GetElement(curField.ConfigParentName);
                if (curFieldParent == null)
                    continue;
                
                list.Add(new Tuple<Code.AddInUtil.UIElement, string>(curFieldParent, curField.Key));
            }
            return list;
        }
        public void SwapUiIndexies(string keyA, string keyB)
        {
            var keyAField = this.Config.TreeRoot.GetKey(keyA, ConfigField.KeyMode.ThrowOnNotFound);
            var keyBField = this.Config.TreeRoot.GetKey(keyB, ConfigField.KeyMode.ThrowOnNotFound);
            if (keyAField.Parent != keyBField.Parent)
                throw new ArgumentException("Parents do not matcH");
            keyAField.Parent.SwapKeyIndexies(keyAField.Name, keyBField.Name);
            this.RegenerateDisplay();
        }
    }
}
