﻿using System;
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

namespace ArmA_UI_Editor.UI.Snaps
{
    /// <summary>
    /// Interaction logic for EditingWindow.xaml
    /// </summary>
    public partial class EditingWindow : Page
    {
        public SQF.ClassParser.File ConfigFile;
        private bool ConfigTextboxDiffersConfigInstance;



        private class TAG_CanvasChildElement
        {
            internal object data;
            internal Code.AddInUtil.UIElement file;
            internal bool isMoveOperation;

            public string FullyQualifiedPath { get; internal set; }
        }


        public EditingWindow()
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
            sb.AppendLine("\t\tclass MyFirstRscText : RscText\r\n\t\t{\r\n\t\t\th = 64;\r\n\t\t\ttext = \"My UI Starts here <3\";\r\n\t\t\tcolorBackground[] = {0.5, 0.1, 0.1, 0.1};\r\n\t\t};");
            sb.AppendLine("\t};");
            sb.AppendLine("};");

            this.Textbox.Text = sb.ToString();
            this.ReinitConfigFileField();
        }

        #region XAML Event Handler
        private void DisplayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void DisplayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void DisplayCanvas_MouseMove(object sender, MouseEventArgs e)
        {

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
            var data = (e.Data.GetData("UiElementsListBoxData") as Code.UI.DragDrop.UiElementsListBoxData);
            var d = new SQF.ClassParser.Data(new SQF.ClassParser.ConfigClass(data.ElementData.ClassFile[0]));

            var mousePos = e.GetPosition(this.DisplayCanvas);
            
            d.Class["x"] = new SQF.ClassParser.Data(mousePos.X, "x");
            d.Class["y"] = new SQF.ClassParser.Data(mousePos.Y, "y");

            string name = d.Name = "My" + data.ElementData.ClassFile.ElementAt(0).Key;
            int count = 0;
            var targetClass = this.ConfigFile[this.ConfigFile.Count - 1].Class["controls"].Class;
            while(targetClass.ContainsKey(name))
            {
                count++;
                name = d.Name + count.ToString();
            }
            d.Name = name;
            targetClass.Add(d.Name, d);
            this.ConfigTextboxDiffersConfigInstance = true;
            this.WriteConfigToScreen();
            this.RegenerateDisplay();
        }


        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ConfigTextboxDiffersConfigInstance = true;
        }

        private void TabControlMainView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TabControlMainView.SelectedIndex == 1)
                this.RegenerateDisplay();
        }
        #endregion

        public void AddElementToDisplay(FrameworkElement el, SQF.ClassParser.Data data, Rect position)
        {

        }

        public void RegenerateDisplay()
        {
            if (this.ConfigTextboxDiffersConfigInstance)
            {
                this.ReinitConfigFileField();
            }
            this.DisplayCanvas.Children.Clear();
            try
            {
                var data = this.ConfigFile[this.ConfigFile.Count - 1];
                Code.Markup.BindConfig.CurrentConfig = this.ConfigFile;
                var uiElements = data.Class["controls"];
                if (uiElements != null && uiElements.IsClass)
                {
                    var controls = uiElements.Class;
                    foreach (var pair in controls)
                    {
                        if (pair.Value.Class.Parent == null)
                        {
                            throw new Exception("Cannot create new elements from scratch in this version of 'ArmA UI Editor'");
                        }
                        var file = AddInManager.Instance.ConfigNameFileDictionary[pair.Value.Class.Parent.Name];
                        using (FileStream stream = System.IO.File.Open(file.__XamlPath, FileMode.Open))
                        {
                            Code.Markup.BindConfig.CurrentClassPath = '/' + data.Name + "/controls/" + pair.Value.Name;
                            var el = (FrameworkElement)System.Windows.Markup.XamlReader.Load(stream);
                            this.DisplayCanvas.Children.Add(el);
                            SQF.ClassParser.Data[] sizeList = new SQF.ClassParser.Data[] {
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/x"),
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/y"),
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/w"),
                                this.ConfigFile.ReceiveFieldFromHirarchy(Code.Markup.BindConfig.CurrentClassPath, "/h")
                            };
                            Canvas.SetLeft(el, sizeList[0].Number);
                            Canvas.SetTop(el, sizeList[1].Number);
                            Canvas.SetRight(el, sizeList[0].Number + sizeList[2].Number);
                            Canvas.SetBottom(el, sizeList[1].Number + sizeList[3].Number);
                            el.Width = sizeList[2].Number;
                            el.Height = sizeList[3].Number;
                            el.Tag = new TAG_CanvasChildElement { data = this.ConfigFile[Code.Markup.BindConfig.CurrentClassPath], file = file, isMoveOperation = false, FullyQualifiedPath = Code.Markup.BindConfig.CurrentClassPath };
                            //ToDo: When Property Panel SnapIn is finalized, reimplement rebind mechanic
                            //if (this.CurPropertyPath == Code.Markup.BindConfig.CurrentClassPath)
                            //{
                            //    currentSelected = el;
                            //    LoadProperties(file.Properties, configFile[Code.Markup.BindConfig.CurrentClassPath]);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Could not regenerate config", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.DisplayCanvas.Children.Clear();
                Frame frame = new Frame();
                frame.Content = new ParseError();
                this.DisplayCanvas.Children.Add(frame);
            }
        }
        public void ReinitConfigFileField()
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

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Parse Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void WriteConfigToScreen()
        {
            using (var memStream = new MemoryStream())
            {
                this.ConfigFile[this.ConfigFile.Count - 1].WriteOut(new StreamWriter(memStream));
                memStream.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new StreamReader(memStream);
                this.Textbox.Text = reader.ReadToEnd();
            }
        }
    }
}