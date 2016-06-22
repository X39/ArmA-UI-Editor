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

namespace ArmA_UI_Editor.UI.Snaps
{
    /// <summary>
    /// Interaction logic for EditingWindow.xaml
    /// </summary>
    public partial class EditingSnap : Page, Code.Interface.ISnapWindow
    {
        #region Events
        public event EventHandler OnUiElementsChanged;
        #endregion
        public SQF.ClassParser.File ConfigFile;
        private bool ConfigTextboxDiffersConfigInstance;
        private bool BlockWriteout;
        private SelectionOverlay SelectionOverlay_ToMove;

        public static readonly DependencyProperty SnapDisabledProperty = DependencyProperty.Register("SnapDisabled", typeof(bool), typeof(EditingSnap));
        public bool SnapEnabled { get { return (bool)GetValue(SnapDisabledProperty); } set { SetValue(SnapDisabledProperty, value); } }
        public static readonly DependencyProperty BackgroundEnabledProperty = DependencyProperty.Register("BackgroundEnabled", typeof(bool), typeof(EditingSnap));
        public bool BackgroundEnabled { get { return (bool)GetValue(BackgroundEnabledProperty); } set { SetValue(BackgroundEnabledProperty, value); } }

        public static readonly DependencyProperty SnapGridProperty = DependencyProperty.Register("SnapGrid", typeof(Rect), typeof(EditingSnap));
        public int SnapGrid { get { return (int)((Rect)GetValue(SnapGridProperty)).Width; } set { SetValue(SnapGridProperty, new Rect(0, 0, value, value)); } }

        public static readonly DependencyProperty ViewScaleProperty = DependencyProperty.Register("ViewScale", typeof(double), typeof(EditingSnap));
        public double ViewScale { get { return (double)GetValue(ViewScaleProperty); } set { SetValue(ViewScaleProperty, value); } }

        public string FilePath { get; set; }

        private bool HasChanges;


        public void UnloadSnap()
        {
            if (HasChanges)
            {
                if (MessageBox.Show("Do you want to save?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
            }
        }

        public void LoadSnap()
        {

        }

        internal class TAG_CanvasChildElement
        {
            internal SQF.ClassParser.Data data;
            internal Code.AddInUtil.UIElement file;
            internal EditingSnap Window;

            internal EditingSnap Owner { get; set; }
            internal string FullyQualifiedPath { get; set; }

            internal void LoadProperties()
            {
                PropertySnap pWindow = PropertySnap.GetDisplayWindow();
                pWindow.LoadProperties(this.file.Properties, data, Window);
            }
        }

        internal void Redraw()
        {
            switch (this.TabControlMainView.SelectedIndex)
            {
                case 0:
                    this.WriteConfigToScreen();
                    break;
                case 1:
                    this.WriteConfigToScreen();
                    this.RegenerateDisplay();
                    break;
            }
        }

        public EditingSnap()
        {
            InitializeComponent();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("class NewArmAUI");
            sb.AppendLine("{");
            sb.AppendLine("\tidd = -1;");
            sb.AppendLine("\tonLoad = \"\";");
            sb.AppendLine("\tonUnload = \"\";");
            sb.AppendLine("\tduration = 32000;");
            sb.AppendLine("\tfadeIn = 0;");
            sb.AppendLine("\tfadeOut = 0;");
            sb.AppendLine("\tenableSimulation = 1;");
            sb.AppendLine("\tclass controls");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tclass MyFirstRscText : RscText\r\n\t\t{\r\n\t\t\tw = \"(120 / 1920) * SafeZoneW\";\r\n\t\t\th = \"(30 / 1080) * SafeZoneH\";\r\n\t\t\ttext = \"My UI Starts here <3\";\r\n\t\t\tcolorBackground[] = {0.1, 0.1, 0.1, 0.5};\r\n\t\t};");
            sb.AppendLine("\t};");
            sb.AppendLine("};");

            this.Textbox.Text = sb.ToString();
            this.ReinitConfigFileField();
            SelectionOverlay_ToMove = null;
            SnapEnabled = false;
            BackgroundEnabled = false;
            SnapGrid = 15;
            ViewScale = 1;
            FilePath = string.Empty;
            HasChanges = true;
        }
        public EditingSnap(string FilePath)
        {
            InitializeComponent();
            using (var reader = new StreamReader(FilePath))
            {
                this.Textbox.Text = reader.ReadToEnd();
            }

            this.ReinitConfigFileField();
            SelectionOverlay_ToMove = null;
            SnapEnabled = false;
            BackgroundEnabled = false;
            SnapGrid = 15;
            ViewScale = 1;
            this.FilePath = FilePath;
            HasChanges = false;
        }
        public string GetFileName()
        {
            return string.IsNullOrWhiteSpace(this.FilePath) ? this.ConfigFile[this.ConfigFile.Count - 1].Name + ".cpp" : this.FilePath.Substring(this.FilePath.LastIndexOfAny(new char[] { '\\', '/' }));
        }

        public void SaveFile()
        {
            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = this.ConfigFile[this.ConfigFile.Count - 1].Name;
                dlg.DefaultExt = ".cpp";
                dlg.Filter = "ArmA Class (.cpp)|*.cpp";
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
            HasChanges = false;
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
            var data = this.ConfigFile[this.ConfigFile.Count - 1];
            var uiElements = data.Class["controls"];
            var tagData = el.Tag as TAG_CanvasChildElement;
            uiElements.Class.Remove(tagData.data.Name);
            this.ConfigTextboxDiffersConfigInstance = true;
            DisplayCanvas.Children.Remove(el);
        }

        #region SelectionOverlay Event Handler
        private void SelectionOverlay_OnStopMove(object sender, EventArgs e)
        {
            SelectionOverlay_ToMove = null;
        }
        private void SelectionOverlay_OnStartMove(object sender, MouseEventArgs e)
        {
            SelectionOverlay_ToMove = sender as SelectionOverlay;
            SelectionOverlay_ToMove.Tag = e.GetPosition(DisplayCanvas);
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
            e.Element.SetCanvasMetrics(metrics);
            if (e.Element is FrameworkElement)
            {
                var fElement = e.Element as FrameworkElement;
                fElement.Width = metrics.Width;
                fElement.Height = metrics.Height;
                var data = fElement.Tag as TAG_CanvasChildElement;
                if (data == null)
                    return;

                if (posChangeNeeded)
                {
                    var field_X = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data.data, "/x", true);
                    var field_Y = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data.data, "/y", true);

                    field_X.String = ToSqfString(FieldTypeEnum.XField, metrics.X);
                    field_Y.String = ToSqfString(FieldTypeEnum.YField, metrics.Y);
                }
                var field_W = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data.data, "/w", true);
                var field_H = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data.data, "/h", true);
                field_W.String = ToSqfString(FieldTypeEnum.WField, metrics.Width);
                field_H.String = ToSqfString(FieldTypeEnum.HField, metrics.Height);
                this.ConfigTextboxDiffersConfigInstance = true;
            }
        }
        private void SelectionOverlay_OnElementMove(object sender, SelectionOverlay.MoveEventArgs e)
        {
            var metrics = e.Element.GetCanvasMetrics();
            metrics.X += e.DeltaX;
            metrics.Y += e.DeltaY;
            e.Element.SetCanvasMetrics(metrics);
            if (e.Element is FrameworkElement)
            {
                var fElement = e.Element as FrameworkElement;
                var data = fElement.Tag as TAG_CanvasChildElement;
                if (data == null)
                    return;

                var field_X = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data.data, "/x", true);
                var field_Y = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data.data, "/y", true);
                field_X.String = ToSqfString(FieldTypeEnum.XField, metrics.X);
                field_Y.String = ToSqfString(FieldTypeEnum.YField, metrics.Y);
                this.ConfigTextboxDiffersConfigInstance = true;
            }
        }
        private void SelectionOverlay_OnOperationFinalized(object sender, FrameworkElement e)
        {
            if (PropertySnap.HasDisplayWindow() && PropertySnap.GetDisplayWindow().CurrentProperties == (e.Tag as TAG_CanvasChildElement).file.Properties)
            {
                (e.Tag as TAG_CanvasChildElement).LoadProperties();
            }
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
                        this.DisplayCanvas.Children.Remove(overlay);
                    }
                }
                return;
            }



            if (overlay == null)
            {
                overlay = this.CreateOrGetSelectionOverlay();
                this.DisplayCanvas.Children.Add(overlay);
                overlay.ToggleElement(thisElement);
                (thisElement.Tag as TAG_CanvasChildElement).LoadProperties();
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (!overlay.ToggleElement(thisElement))
                {
                    this.DisplayCanvas.Children.Remove(overlay);
                }
            }
            else
            {
                var overlayMetrics = overlay.GetCanvasMetrics();
                var mousePosRelToOverlay = e.GetPosition(overlay);
                if (mousePosRelToOverlay.X > overlayMetrics.Width || mousePosRelToOverlay.X < 0 || mousePosRelToOverlay.Y > overlayMetrics.Height || mousePosRelToOverlay.Y < 0)
                {
                    this.DisplayCanvas.Children.Remove(overlay);
                    overlay = this.CreateOrGetSelectionOverlay();
                    this.DisplayCanvas.Children.Add(overlay);
                    overlay.ToggleElement(thisElement);
                    (thisElement.Tag as TAG_CanvasChildElement).LoadProperties();
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
            System.Windows.UIElement thisElement = null;
            SelectionOverlay overlay = null;
            for (int i = arr.Count - 1; i >= 0; i--)
            {
                var it = arr[i];
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
            if (SelectionOverlay_ToMove != null)
            {
                var pos = e.GetPosition(DisplayCanvas);
                var oldPos = (Point)SelectionOverlay_ToMove.Tag;
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
                    SelectionOverlay_ToMove.Tag = pos;
                    SelectionOverlay_ToMove.DoMove(deltaX, deltaY);
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
                return;
            var data = (e.Data.GetData("UiElementsListBoxData") as Code.UI.DragDrop.UiElementsListBoxData);
            var d = new SQF.ClassParser.Data(new SQF.ClassParser.ConfigClass(data.ElementData.ClassFile[0]));

            var mousePos = e.GetPosition(this.DisplayCanvas);

            d.Class["x"] = new SQF.ClassParser.Data(mousePos.X, "x");
            d.Class["y"] = new SQF.ClassParser.Data(mousePos.Y, "y");

            string name = d.Name = "My" + data.ElementData.ClassFile.ElementAt(0).Key;
            int count = 0;
            var targetClass = this.ConfigFile[this.ConfigFile.Count - 1].Class["controls"].Class;
            while (targetClass.ContainsKey(name))
            {
                count++;
                name = d.Name + count.ToString();
            }
            d.Name = name;
            targetClass.Add(d.Name, d);
            this.ConfigTextboxDiffersConfigInstance = true;
            this.WriteConfigToScreen();
            this.RegenerateDisplay();
            this.ThisScrollViewer.Focus();
            if (OnUiElementsChanged != null)
                OnUiElementsChanged(this, new EventArgs());
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
                this.WriteConfigToScreen();
                if (this.ReinitConfigFileField())
                    this.SaveFile();
            }
        }


        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasChanges = true;
            this.BlockWriteout = false;
            this.ConfigTextboxDiffersConfigInstance = true;
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
        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (this.ReinitConfigFileField())
                    this.SaveFile();
                e.Handled = true;
            }
        }
        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            // this.ThisScrollViewer.Focus();
        }
        private void TabControlMainView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TabControlMainView.SelectedIndex == 1)
            {
                this.RegenerateDisplay();
            }
            else if (this.TabControlMainView.SelectedIndex == 0 && this.ConfigTextboxDiffersConfigInstance)
            {
                this.ConfigTextboxDiffersConfigInstance = false;
                WriteConfigToScreen();
            }
        }
        private void TabControlMainView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.WriteConfigToScreen();
                if (this.ReinitConfigFileField())
                    this.SaveFile();
                e.Handled = true;
            }
        }
        #endregion

        public void AddElementToDisplay(FrameworkElement el, SQF.ClassParser.Data data, Rect position)
        {
            throw new NotImplementedException();
        }

        public bool TryRefreshAll()
        {
            if(ConfigTextboxDiffersConfigInstance)
                return WriteConfigToScreen(true) && RegenerateDisplay();
            else
                return RegenerateDisplay() && WriteConfigToScreen(true);
        }

        public bool RegenerateDisplay()
        {
            var mainWindow = App.Current.MainWindow as MainWindow;
            try
            {
                if (this.ConfigTextboxDiffersConfigInstance || this.BlockWriteout)
                {
                    if (this.BlockWriteout || !this.ReinitConfigFileField())
                    {
                        mainWindow.StatusBar.Background = App.Current.Resources["SCB_UIRed"] as SolidColorBrush;
                        mainWindow.StatusTextbox.Text = App.Current.Resources["STR_CODE_EditingWindow_ConfigParsingError"] as String;
                        this.DisplayCanvas.Children.Clear();
                        Frame frame = new Frame();
                        frame.Content = new ParseError();
                        this.DisplayCanvas.Children.Add(frame);
                        BlockWriteout = true;
                        return false;
                    }
                }
                int initialCount = 0;
                foreach (var it in this.DisplayCanvas.Children)
                {
                    if (it is SelectionOverlay)
                        continue;
                    initialCount++;
                }
                this.DisplayCanvas.Children.Clear();
                var data = this.ConfigFile[this.ConfigFile.Count - 1];
                Code.Markup.BindConfig.CurrentConfig = this.ConfigFile;
                var uiElements = data.Class["controls"];
                if (uiElements != null && uiElements.IsClass)
                {
                    var controls = uiElements.Class;
                    int index = 0;
                    foreach (var pair in controls)
                    {
                        if (pair.Value.Class.Parent == null)
                        {
                            throw new Exception(string.Format("Cannot create new elements from scratch in this version of 'ArmA UI Editor'\nmissing parent of '{0}' in AddIns", pair.Value.Name));
                        }
                        var file = AddInManager.Instance.GetElement(pair.Value.Class.Parent.Name);
                        using (FileStream stream = System.IO.File.OpenRead(file.__XamlPath))
                        {

                            Code.Markup.BindConfig.CurrentClassPath = '/' + data.Name + "/controls/" + pair.Value.Name;

                            ArmA_UI_Editor.Code.Markup.BindConfig.CurrentPath = file.Parent.ThisPath;

                            var el = (FrameworkElement)System.Windows.Markup.XamlReader.Load(stream);
                            this.DisplayCanvas.Children.Add(el);
                            SQF.ClassParser.Data[] sizeList = new SQF.ClassParser.Data[] {
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/x"),
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/y"),
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/w"),
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/h")
                            };
                            var tmp = sizeList[0].IsNumber ? sizeList[0].Number : FromSqfString(FieldTypeEnum.XField, sizeList[0].String);
                            Canvas.SetLeft(el, tmp);
                            tmp += sizeList[2].IsNumber ? sizeList[2].Number : FromSqfString(FieldTypeEnum.WField, sizeList[2].String);
                            Canvas.SetRight(el, tmp);
                            tmp = sizeList[1].IsNumber ? sizeList[1].Number : FromSqfString(FieldTypeEnum.YField, sizeList[1].String);
                            Canvas.SetTop(el, tmp);
                            tmp += sizeList[3].IsNumber ? sizeList[3].Number : FromSqfString(FieldTypeEnum.HField, sizeList[3].String);
                            Canvas.SetBottom(el, tmp);
                            Canvas.SetZIndex(el, index);

                            tmp = sizeList[2].IsNumber ? sizeList[2].Number : FromSqfString(FieldTypeEnum.WField, sizeList[2].String);
                            el.Width = tmp;
                            tmp = sizeList[3].IsNumber ? sizeList[3].Number : FromSqfString(FieldTypeEnum.HField, sizeList[3].String);
                            el.Height = tmp;
                            el.Tag = new TAG_CanvasChildElement { data = this.ConfigFile[Code.Markup.BindConfig.CurrentClassPath], file = file, FullyQualifiedPath = Code.Markup.BindConfig.CurrentClassPath, Owner = this, Window = this };
                            if (PropertySnap.HasDisplayWindow() && PropertySnap.GetDisplayWindow().CurrentData != null && PropertySnap.GetDisplayWindow().CurrentData.Name == (el.Tag as TAG_CanvasChildElement).data.Name)
                            {
                                var overlay = this.CreateOrGetSelectionOverlay(false);
                                this.DisplayCanvas.Children.Add(overlay);
                                overlay.ToggleElement(el);
                                (el.Tag as TAG_CanvasChildElement).LoadProperties();
                            }
                        }
                        index++;
                    }
                }
                mainWindow.StatusBar.Background = App.Current.Resources["SCB_UIBlue"] as SolidColorBrush;
                mainWindow.StatusTextbox.Text = "";
                foreach (var it in this.DisplayCanvas.Children)
                {
                    initialCount--;
                }
                if (initialCount != 0 && this.OnUiElementsChanged != null)
                    OnUiElementsChanged(this, new EventArgs());
                return true;
            }
            catch (SQF.ClassParser.File.ParseException ex)
            {
                Logger.Instance.log(Logger.LogLevel.ERROR, ex.Message);
                mainWindow.StatusBar.Background = App.Current.Resources["SCB_UIRed"] as SolidColorBrush;
                mainWindow.StatusTextbox.Text = App.Current.Resources["STR_CODE_EditingWindow_ConfigParsingError"] as String;
                this.DisplayCanvas.Children.Clear();
                Frame frame = new Frame();
                frame.Content = new ParseError();
                this.DisplayCanvas.Children.Add(frame);
            }
            catch (Exception ex)
            {
                Logger.Instance.log(Logger.LogLevel.ERROR, ex.Message);
                mainWindow.StatusBar.Background = App.Current.Resources["SCB_UIRed"] as SolidColorBrush;
                mainWindow.StatusTextbox.Text = App.Current.Resources["STR_CODE_EditingWindow_ConfigParsingError"] as String;
                this.DisplayCanvas.Children.Clear();
                Frame frame = new Frame();
                frame.Content = new ParseError();
                this.DisplayCanvas.Children.Add(frame);
            }
            return false;
        }
        public bool ReinitConfigFileField()
        {
            try
            {
                using (Stream stream = this.Textbox.Text.ToStream())
                {
                    this.ConfigFile = new SQF.ClassParser.File();
                    this.ConfigFile.AppendConfig(AddInManager.Instance.MainFile);
                    this.ConfigFile.AppendConfig(stream);
                }
                this.ConfigTextboxDiffersConfigInstance = false;
                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow.SetStatusbarText("", false);
                return true;
            }
            catch (Exception ex)
            {
                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow.SetStatusbarText(App.Current.Resources["STR_CODE_EditingWindow_ConfigParsingError"] as String, true);
                Logger.Instance.log(Logger.LogLevel.ERROR, ex.Message);
            }
            return false;
        }
        private bool WriteConfigToScreen(bool force = false)
        {
            if (BlockWriteout && !force)
                return false;
            using (var memStream = new MemoryStream())
            {
                this.ConfigFile[this.ConfigFile.Count - 1].WriteOut(new StreamWriter(memStream));
                memStream.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new StreamReader(memStream);
                this.Textbox.Text = reader.ReadToEnd();
            }
            return true;
        }
        private SelectionOverlay CreateOrGetSelectionOverlay(bool mouseDownOnCreate = true)
        {

            SelectionOverlay el = null;
            foreach(var it in this.DisplayCanvas.Children)
            {
                if(it is SelectionOverlay)
                {
                    el = it as SelectionOverlay;
                    break;
                }
            }
            if (el == null)
            {
                el = new SelectionOverlay(mouseDownOnCreate);
                el.OnElementMove += SelectionOverlay_OnElementMove;
                el.OnElementResize += SelectionOverlay_OnElementResize;
                el.OnStartMove += SelectionOverlay_OnStartMove;
                el.OnStopMove += SelectionOverlay_OnStopMove;
                el.OnOperationFinalized += SelectionOverlay_OnOperationFinalized;
                Canvas.SetZIndex(el, 10000);
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

                    metricts = Code.Utility.GetCanvasMetrics(el);
                    tmp = metricts.X % snapGrid;
                    deltaX = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                    tmp = metricts.Y % snapGrid;
                    deltaY = tmp > snapGrid / 2 ? snapGrid - tmp : -tmp;
                    if (deltaX != 0 || deltaY != 0)
                        SelectionOverlay_OnElementResize(cm, new SelectionOverlay.ResizeEventArgs(SelectionOverlay.ResizeEventArgs.Direction.TopLeft, -deltaX, -deltaY, el));
                }
            }
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
                    var tag = el.Tag as TAG_CanvasChildElement;
                    tag.LoadProperties();
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

        public List<Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>> GetUiElements()
        {
            var list = new List<Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>>();
            if (ConfigTextboxDiffersConfigInstance)
                TryRefreshAll();
            var data = this.ConfigFile[this.ConfigFile.Count - 1];
            var uiElements = data.Class["controls"];
            var controls = uiElements.Class;
            foreach (var pair in controls)
            {
                list.Add(new Tuple<Code.AddInUtil.UIElement, KeyValuePair<string, Data>>(AddInManager.Instance.GetElement(pair.Value.Class.Parent.Name), pair));
            }
            return list;
        }
        public void SwapUiIndexies(string name1, string name2)
        {
            TryRefreshAll();
            var data = this.ConfigFile[this.ConfigFile.Count - 1];
            var uiElements = data.Class["controls"];
            var controls = uiElements.Class;
            int index1 = -1;
            int index2 = -1;
            for (int i = 0; i < controls.Count; i++)
            {
                var it = controls.ElementAt(i);
                if (it.Key == name1)
                {
                    index1 = i;
                }
                else if (it.Key == name2)
                {
                    index2 = i;
                }
                if (index1 != -1 && index2 != -1)
                {
                    break;
                }
            }
            if (index1 == -1 || index2 == -1)
                throw new ArgumentException("Cannot find given classname(s)");
            uiElements.Class = new ConfigClass();
            for (int i = 0; i < controls.Count; i++)
            {
                var it = controls.ElementAt(i);
                if (i == index1)
                {
                    it = controls.ElementAt(index2);
                }
                else if(i == index2)
                {
                    it = controls.ElementAt(index1);
                }
                uiElements.Class.Add(it.Key, it.Value);
            }
            TryRefreshAll();
        }
    }
}
