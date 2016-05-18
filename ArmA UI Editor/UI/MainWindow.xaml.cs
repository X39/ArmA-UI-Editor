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
using SQF.ClassParser;

namespace ArmA_UI_Editor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class CanvasDataTag
        {
            public Data data;
            public Code.AddInUtil.File file;
            public string FullyQualifiedPath;
            internal bool isMoveOperation;
            internal Point lastClickPos;
        }
        private class CanvasTag
        {
            public bool ConfigHasChanged;
            public Data Element;
        }
        private string CurPropertyPath = string.Empty;
        SQF.ClassParser.File configFile;
        public MainWindow()
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
            sb.AppendLine("\t");
            sb.AppendLine("\tclass controls");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tclass MyFirstRscText : RscText\r\n\t\t{\r\n\t\t\ttext = \"My UI Starts here <3\";\r\n\t\t\tcolorBackground[] = {0.5, 0.1, 0.1, 0.1};\r\n\t\t};");
            sb.AppendLine("\t};");
            sb.AppendLine("};");
            this.ConfigTextbox.Text = sb.ToString();
            ConfigRenderCanvas.Tag = new CanvasTag { ConfigHasChanged = false, Element = null };
            Code.AddInUtil.Properties.Property.PType.ValueChanged += PType_ValueChanged;
            this.RegenerateCurrentView();
        }

        private void PType_ValueChanged(object sender, EventArgs e)
        {
            using (var memStream = new MemoryStream())
            {
                (ConfigRenderCanvas.Tag as CanvasTag).Element.WriteOut(new StreamWriter(memStream));
                memStream.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new StreamReader(memStream);
                ConfigTextbox.Text = reader.ReadToEnd();
            }
            RegenerateCurrentView();
        }

        private void ListView_Initialized(object sender, EventArgs e)
        {
            ListView lw = sender as ListView;
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
            lw.Items.Add(new { Text = "FOOBAR", Object = "FOO" });
        }

        private void ConfigTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
            }
            catch(Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
            
        }

        private void ToolBox_Initialized(object sender, EventArgs e)
        {
            foreach(var addIn in Code.AddInManager.Instance.AddIns)
            {
                foreach(var it in addIn.Files)
                {
                    this.ToolBox.Items.Add(it);
                }
            }
        }

        private Point? ToolBoxLastMouseDown;
        private void ToolBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToolBoxLastMouseDown = e.GetPosition(null);
        }

        private void ToolBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ToolBoxLastMouseDown.HasValue)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = ToolBoxLastMouseDown.Value - mousePos;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    ToolBoxLastMouseDown = null;
                    DragDrop.DoDragDrop(sender as ListBox, new DataObject("File", (Code.AddInUtil.File)(sender as ListBox).SelectedItem), DragDropEffects.Move);
                    e.Handled = true;
                }
            }
        }

        private void ConfigRenderCanvas_DragOver(object sender, DragEventArgs e)
        {
        }

        private void ConfigRenderCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent("File"))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private int upCounter = 0;
        private void ConfigRenderCanvas_Drop(object sender, DragEventArgs e)
        {
            Code.AddInUtil.File file = (e.Data.GetData("File") as Code.AddInUtil.File);
            using (FileStream stream = System.IO.File.Open(file.__XamlPath, FileMode.Open))
            {
                try
                {
                    Code.Markup.BindConfig.CurrentConfig = file.ClassFile;
                    Code.Markup.BindConfig.CurrentClassPath = '/' + file.ClassFile.ElementAt(0).Key;
                    var el = (FrameworkElement)System.Windows.Markup.XamlReader.Load(stream);
                    ConfigRenderCanvas.Children.Add(el);
                    ConfigClass cc = new ConfigClass(file.ClassFile.ElementAt(0).Value);
                    Data d = new Data(cc);
                    d.Name = "My" + file.ClassFile.ElementAt(0).Key + upCounter++.ToString();
                    configFile[configFile.Count - 1].Class["controls"].Class.Add(d.Name, d);

                    SQF.ClassParser.Data[] sizeList = new Data[] {
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/x"),
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/y"),
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/w"),
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/h")
                            };
                    Canvas.SetLeft(el, sizeList[0].Number);
                    Canvas.SetTop(el, sizeList[1].Number);
                    Canvas.SetRight(el, sizeList[0].Number + sizeList[2].Number);
                    Canvas.SetBottom(el, sizeList[1].Number + sizeList[3].Number);
                    el.Width = sizeList[2].Number;
                    el.Height = sizeList[3].Number;
                    el.MouseLeftButtonDown += CanvasChild_MouseLeftButtonDown;
                    el.MouseLeftButtonUp += CanvasChild_MouseLeftButtonUp;
                    el.MouseMove += CanvasChild_MouseMove;
                    el.Tag = new CanvasDataTag { data = d, file = file, isMoveOperation = false };

                    (ConfigRenderCanvas.Tag as CanvasTag).ConfigHasChanged = true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, "XAML Parse Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void RegenerateCurrentView()
        {
            ConfigRenderCanvas.Children.Clear();
            using (Stream stream = this.ConfigTextbox.Text.ToStream())
            {
                configFile = new SQF.ClassParser.File();
                configFile.AppendConfig(AddInManager.Instance.MainFile);
                configFile.AppendConfig(stream);
                (ConfigRenderCanvas.Tag as CanvasTag).Element = configFile[configFile.Count - 1];
                Code.Markup.BindConfig.CurrentConfig = configFile;
            }
            var data = this.configFile[this.configFile.Count - 1];
            var uiElements = data.Class["controls"];
            if(uiElements != null && uiElements.IsClass)
            {
                var controls = uiElements.Class;
                foreach(var pair in controls)
                {
                    if(pair.Value.Class.Parent == null)
                    {
                        throw new Exception("Cannot create new elements from scratch in this version of 'ArmA UI Editor'");
                    }
                    var file = AddInManager.Instance.ConfigNameFileDictionary[pair.Value.Class.Parent.Name];
                    using (FileStream stream = System.IO.File.Open(file.__XamlPath, FileMode.Open))
                    {
                        try
                        {
                            Code.Markup.BindConfig.CurrentClassPath = '/' + data.Name + "/controls/" + pair.Value.Name;
                            var el = (FrameworkElement)System.Windows.Markup.XamlReader.Load(stream);
                            ConfigRenderCanvas.Children.Add(el);
                            SQF.ClassParser.Data[] sizeList = new Data[] {
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/x"),
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/y"),
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/w"),
                                this.configFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/h")
                            };
                            Canvas.SetLeft(el, sizeList[0].Number);
                            Canvas.SetTop(el, sizeList[1].Number);
                            Canvas.SetRight(el, sizeList[0].Number + sizeList[2].Number);
                            Canvas.SetBottom(el, sizeList[1].Number + sizeList[3].Number);
                            el.Width = sizeList[2].Number;
                            el.Height = sizeList[3].Number;
                            el.MouseLeftButtonDown += CanvasChild_MouseLeftButtonDown;
                            el.MouseLeftButtonUp += CanvasChild_MouseLeftButtonUp;
                            el.MouseMove += CanvasChild_MouseMove;
                            el.Tag = new CanvasDataTag { data = configFile[Code.Markup.BindConfig.CurrentClassPath], file = file, isMoveOperation = false, FullyQualifiedPath = Code.Markup.BindConfig.CurrentClassPath };
                            if(this.CurPropertyPath == Code.Markup.BindConfig.CurrentClassPath)
                            {
                                LoadProperties(file.Properties, configFile[Code.Markup.BindConfig.CurrentClassPath]);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Cannot Regenerate config", ex);
                        }
                    }
                }
            }
        }

        private void CanvasChild_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var el = (FrameworkElement)sender;
            CanvasDataTag tag = (CanvasDataTag)el.Tag;
            tag.lastClickPos = e.GetPosition(ConfigRenderCanvas);
            foreach(var it in ConfigRenderCanvas.Children)
            {
                (it as UIElement).IsHitTestVisible = false;
            }
            el.IsHitTestVisible = true;
        }
        private void CanvasChild_MouseMove(object sender, MouseEventArgs e)
        {
            var el = (FrameworkElement)sender;
            CanvasDataTag tag = (CanvasDataTag)el.Tag;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                tag.isMoveOperation = true;
                var curPos = e.GetPosition(ConfigRenderCanvas);
                var deltaX = tag.lastClickPos.X - curPos.X;
                var deltaY = tag.lastClickPos.Y - curPos.Y;
                tag.lastClickPos = curPos;
                var left = Canvas.GetLeft(el) - deltaX;
                var top = Canvas.GetTop(el) - deltaY;
                var right = Canvas.GetRight(el) - deltaX;
                var bottom = Canvas.GetBottom(el) - deltaY;
                if (left < 0 || top < 0 || right > ConfigRenderCanvas.ActualWidth || bottom > ConfigRenderCanvas.ActualHeight)
                    return;
                Canvas.SetLeft(el, left);
                Canvas.SetTop(el, top);
                Canvas.SetRight(el, right);
                Canvas.SetBottom(el, bottom);
                if (!tag.data.Class.ContainsKey("x"))
                    tag.data.Class["x"] = new Data(left, "x");
                else
                    tag.data.Class["x"].Number = left;

                if (!tag.data.Class.ContainsKey("y"))
                    tag.data.Class["y"] = new Data(top, "y");
                else
                    tag.data.Class["y"].Number = top;

                (ConfigRenderCanvas.Tag as CanvasTag).ConfigHasChanged = true;
                e.Handled = true;
            }
        }
        private void CanvasChild_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CanvasDataTag tag = (CanvasDataTag)((FrameworkElement)sender).Tag;
            if(tag.isMoveOperation)
            {
                tag.isMoveOperation = false;
            }
            else
            {
                this.CurPropertyPath = tag.FullyQualifiedPath;
                LoadProperties(tag.file.Properties, tag.data);
            }
            foreach (var it in ConfigRenderCanvas.Children)
            {
                (it as UIElement).IsHitTestVisible = true;
            }
        }
        private void LoadProperties(Code.AddInUtil.Properties properties, Data data)
        {
            this.ElementProperties.Children.Clear();
            foreach (var groupIt in properties.Items)
            {
                var group = new PropertyGroup();
                group.IsExpaned = true;
                group.Header.Text = groupIt.Name;
                this.ElementProperties.Children.Add(group);
                foreach (var property in groupIt.Items)
                {
                    var el = new Property();
                    el.Title.Text = property.DisplayName;
                    Data d = SQF.ClassParser.File.ReceiveFieldFromHirarchy(data, property.FieldPath);
                    var fEl = property.PropertyType.GenerateUiElement(d);
                    el.ConfigElement.Content = fEl;
                    fEl.Tag = new Code.AddInUtil.Properties.Property.PTypeDataTag { File = this.configFile, Path = property.FieldPath, BaseData = data };
                    group.ItemsPanel.Children.Add(el);
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, object ___IGNORE___ = null)
        {
            switch((sender as TabControl).SelectedIndex)
            {
                case 0:
                    RegenerateCurrentView();
                    break;
                case 1:
                    if((ConfigRenderCanvas.Tag as CanvasTag).ConfigHasChanged)
                    {
                        (ConfigRenderCanvas.Tag as CanvasTag).ConfigHasChanged = false;
                        using (var memStream = new MemoryStream())
                        {
                            (ConfigRenderCanvas.Tag as CanvasTag).Element.WriteOut(new StreamWriter(memStream));
                            memStream.Seek(0, SeekOrigin.Begin);
                            StreamReader reader = new StreamReader(memStream);
                            ConfigTextbox.Text = reader.ReadToEnd();
                        }
                    }
                    break;
            }
        }
    }
}
