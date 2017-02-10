using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit.Highlighting;
using VirtualRealityEngine.Config.Control;
using VirtualRealityEngine.Config.Parser;
using System.Windows.Threading;

namespace ArmA.Studio.DataContext
{
    public class UiEditorDocument : TextEditorDocument
    {
        private sealed class ConfigEntryUiControlBinding : UI.CustomBinding<ConfigEntry, ControlBase>
        {
            public ConfigEntryUiControlBinding(PropertyInfo propSource, ConfigEntry source, PropertyInfo propTarget, ControlBase target) : base(propSource, source, propTarget, target) { }
        }
        private static DataTemplate ThisTemplate { get; set; }
        private static IHighlightingDefinition ThisSyntaxName { get; set; }
        static UiEditorDocument()
        {
            ThisSyntaxName = LoadAvalonEditSyntaxFiles(Path.Combine(App.SyntaxFilesPath, "armaconfig.xshd"));
            ThisTemplate = GetDataTemplateFromAssemblyRes("ArmA.Studio.UI.DataTemplates.UiEditorDocumentTemplate.xaml");
        }
        public override string[] SupportedFileExtensions { get { return new string[] { ".uic" }; } }
        public override IHighlightingDefinition SyntaxDefinition { get { return ThisSyntaxName; } }

        public FlowDocument VirtualConfigDocument { get { return this._VirtualConfigDocument; } set { this._VirtualConfigDocument = value; this.RaisePropertyChanged(); } }
        private FlowDocument _VirtualConfigDocument;
        public ConfigEntry ConfigTreeRoot { get { return this._ConfigTreeRoot; } set { this._ConfigTreeRoot = value; this.RaisePropertyChanged(); } }
        private ConfigEntry _ConfigTreeRoot;
        public ObservableCollection<ControlBase> Controls { get { return this._Controls; } set { this._Controls = value; this.RaisePropertyChanged(); } }
        private ObservableCollection<ControlBase> _Controls;
        public override DataTemplate Template { get { return ThisTemplate; } }
        //ToDo: Create Controls DataTemplates

        public int CurrentTabIndex
        {
            get
            {
                return this._CurrentTabIndex;
            }
            set
            {
                this._CurrentTabIndex = value;
                this.RaisePropertyChanged();
                if (value == 0)
                {
                    //ToDo: Clear UI-Controls and ConfigTree
                    if (VirtualConfigDocument != null)
                    {
                        this.Document.Text = new TextRange(this.VirtualConfigDocument.ContentStart, this.VirtualConfigDocument.ContentEnd).Text;
                        this.VirtualConfigDocument = null;
                        this.ConfigTreeRoot = null;
                    }
                }
                else if(value == 1)
                {
                    //ToDo: Build UI-Controls and ConfigTree
                    this.VirtualConfigDocument = new FlowDocument();
                    new TextRange(this.VirtualConfigDocument.ContentStart, this.VirtualConfigDocument.ContentEnd).Text = this.Document.Text;
                    this.ConfigTreeRoot = new ConfigEntry(null) { };
                    this.BuildConfigTree();
                }
            }
        }
        private int _CurrentTabIndex;
        private string ConfigEntryName;

        protected override void OnTextChanged(object param)
        {
            base.OnTextChanged(param);
        }

        public UiEditorDocument()
        {
            
        }

        public void BuildConfigTree()
        {
            using (var memstream = new MemoryStream())
            {
                { //Load content into MemoryStream
                    var writer = new StreamWriter(memstream);
                    writer.Write(this.Document.Text);
                    writer.Flush();
                    memstream.Seek(0, SeekOrigin.Begin);
                }
                //Setup base requirements for the parser
                var parser = new Parser(new Scanner(memstream));

                parser.Root = this.ConfigTreeRoot;
                parser.doc = this.VirtualConfigDocument;
                parser.Parse();
                if(parser.errors.Count > 0)
                {
                    this.VirtualConfigDocument = null;

                    Application.Current.Dispatcher.BeginInvoke((Action)delegate
                    { this.CurrentTabIndex = 0; }, DispatcherPriority.Render, null);

                    //ToDo: Remove textBox and display it different to user
                    MessageBox.Show("SyntaxError");

                    //ToDo: Add error underlining to AvalonEdit
                    //https://github.com/siegfriedpammer/AvalonEditSamples/blob/master/TextMarkerSample/SharpDevelop/TextMarkerService.cs
                    //https://github.com/siegfriedpammer/AvalonEditSamples/blob/master/TextMarkerSample/SharpDevelop/ITextMarker.cs
                    //https://github.com/siegfriedpammer/AvalonEditSamples/blob/master/TextMarkerSample/Window1.xaml.cs
                    return;
                }
                if(this.ConfigTreeRoot.Children.Count > 1)
                {
                    var dlgContext = new Dialogs.ConfigEntrySelectorDialogDataContext();
                    foreach(var entry in this.ConfigTreeRoot.Children)
                    {
                        dlgContext.ThisCollection.Add(entry);
                    }
                    var dlg = new Dialogs.ConfigEntrySelectorDialog(dlgContext);
                    var dlgResult = dlg.ShowDialog();
                    if(!dlgResult.HasValue || !dlgResult.Value)
                    {
                        this.CurrentTabIndex = 0;
                        return;
                    }
                    this.ConfigEntryName = (dlgContext.SelectedValue as ConfigEntry).Name;
                }
                else if(this.ConfigTreeRoot.Children.Count == 0)
                {
                    System.Windows.MessageBox.Show(Properties.Localization.NoConfigPresent_Body, Properties.Localization.NoConfigPresent_Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                else
                {
                    this.ConfigEntryName = this.ConfigTreeRoot.Children.First().Name;
                }
                var controls = this.ConfigTreeRoot[this.ConfigEntryName]["controls"];
                if(controls == null)
                {
                    //ToDo: Inform user that there is no controls class
                    return;
                }
                foreach (var it in controls.Children)
                {
                    //ToDo: Add items to this.Controls
                }
            }
        }
    }
}