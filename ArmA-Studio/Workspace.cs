using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArmA.Studio.UI.Commands;
using System.Reflection;
using Xceed.Wpf.AvalonDock;
using Utility;


namespace ArmA.Studio
{
    public sealed class Workspace : INotifyPropertyChanged
    {
        public const string CONST_DOCKING_MANAGER_LAYOUT_NAME = "docklayout.xml";

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string callerName = "") { this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName)); }

        public static Workspace CurrentWorkspace { get { return _CurrentWorkspace; } set { if (_CurrentWorkspace != null) _CurrentWorkspace.Close(); _CurrentWorkspace = value; value?.Open(); } }
        private static Workspace _CurrentWorkspace;

        public SolutionUtil.Solution CurrentSolution { get { return this._CurrentSolution; } set { this._CurrentSolution = value; this.RaisePropertyChanged(); } }
        private SolutionUtil.Solution _CurrentSolution;
        public UI.ViewModel.IPropertyDatatemplateProvider CurrentSelectedProperty { get { return this._CurrentSelectedProperty; } set { this._CurrentSelectedProperty = value; this.RaisePropertyChanged(); } }
        public DataTemplate CurrentSelectedPropertyTemplate { get { return this._CurrentSelectedProperty?.PropertiesTemplate; } }
        private UI.ViewModel.IPropertyDatatemplateProvider _CurrentSelectedProperty;
        public ObservableCollection<PanelBase> PanelsDisplayed { get { return this._PanelsDisplayed; } set { this._PanelsDisplayed = value; this.RaisePropertyChanged(); this.RaisePropertyChanged("CurrentSelectedPropertyTemplate"); } }
        private ObservableCollection<PanelBase> _PanelsDisplayed;
        public ObservableCollection<PanelBase> AllPanels { get { return this._AllPanels; } set { this._AllPanels = value; this.RaisePropertyChanged(); } }
        private ObservableCollection<PanelBase> _AllPanels;
        public ObservableCollection<DocumentBase> DocumentsDisplayed { get { return this._DocumentsDisplayed; } set { this._DocumentsDisplayed = value; this.RaisePropertyChanged(); } }
        private ObservableCollection<DocumentBase> _DocumentsDisplayed;
        public ObservableCollection<DocumentBase> AvailableDocuments { get { return this._AvailableDocuments; } set { this._AvailableDocuments = value; this.RaisePropertyChanged(); } }
        private ObservableCollection<DocumentBase> _AvailableDocuments;
        public ICommand CmdDisplayPanel { get; private set; }
        public ICommand CmdDisplayLicensesDialog { get; private set; }
        public ICommand CmdDockingManagerInitialized { get; private set; }
        public ICommand CmdDockingManagerUnloaded { get; private set; }
        public ICommand CmdMainWindowClosing { get; private set; }

        public string WorkingDir { get; private set; }

        public DocumentBase GetDocumentOfSolutionFileBase(SolutionUtil.SolutionFileBase sfb)
        {
            foreach (var it in this.DocumentsDisplayed)
            {
                if (it.FilePath == sfb.FullPath)
                {
                    return it;
                }
            }
            return null;
        }

        public Workspace(string path)
        {
            this._AllPanels = new ObservableCollection<PanelBase>(FindAllAnchorablePanelsInAssembly());
            this._PanelsDisplayed = new ObservableCollection<PanelBase>();
            this._DocumentsDisplayed = new ObservableCollection<DocumentBase>();
            this._AvailableDocuments = new ObservableCollection<DocumentBase>(FindAllDocumentsInAssembly());
            this.WorkingDir = path;
            this.CmdDisplayPanel = new RelayCommand((p) =>
            {
                if (p is PanelBase)
                {
                    var pb = p as PanelBase;
                    if (this.PanelsDisplayed.Contains(p))
                    {
                        pb.CurrentVisibility = pb.CurrentVisibility == System.Windows.Visibility.Visible ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        this.PanelsDisplayed.Add(pb);
                    }
                }
            });
            this.CmdDisplayLicensesDialog = new RelayCommand((p) =>
            {
                var dlg = new Dialogs.LicenseViewer();
                var dlgResult = dlg.ShowDialog();
            });
            this.CmdDockingManagerInitialized = new RelayCommand((p) =>
            {
                var dm = p as DockingManager;
                if (dm == null)
                    return;
                LoadLayout(dm, Path.Combine(App.ConfigPath, CONST_DOCKING_MANAGER_LAYOUT_NAME));
                foreach(var panel in AllPanels)
                {
                    if(panel.IsSelected)
                    {
                        this.PanelsDisplayed.Add(panel);
                    }
                }
            });
            this.CmdDockingManagerUnloaded = new RelayCommand((p) =>
            {
                var dm = p as DockingManager;
                if (dm == null)
                    return;
                SaveLayout(dm, Path.Combine(App.ConfigPath, CONST_DOCKING_MANAGER_LAYOUT_NAME));
            });
            this.CmdMainWindowClosing = new RelayCommand((p) => App.Current.Shutdown((int)App.ExitCodes.OK));
        }

        private static void LoadLayout(DockingManager dm, string v)
        {
            if (!File.Exists(v))
            {
                return;
            }
            try
            {
                using (var reader = File.OpenRead(v))
                {
                    var layoutSerializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(dm);
                    layoutSerializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                //ToDo: Log failed layout loading
            }
        }
        private static void SaveLayout(DockingManager dm, string v)
        {
            var dir = Path.GetDirectoryName(v);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (var writer = File.OpenWrite(v))
            {
                var layoutSerializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(dm);
                layoutSerializer.Serialize(writer);
            }
        }

        public void OpenOrFocusDocument(string path)
        {
            var fullPath = Path.Combine(this.WorkingDir, path);
            //Check if document is already open and select instead of open
            foreach (var doc in DocumentsDisplayed)
            {
                if(doc.FilePath == fullPath)
                {
                    doc.IsSelected = true;
                    return;
                }
            }

            Type docType = null;
            var fExt = Path.GetExtension(path);
            foreach (var doc in AvailableDocuments)
            {
                if (doc.SupportedFileExtensions.Contains(fExt))
                {
                    docType = doc.GetType();
                }
            }
            if (docType == null)
            {
                //ToDo: Let user decide how to open this document
                MessageBox.Show("No matching editor context found. Selecting is not yet implemented.");
                return;
            }
            var instance = Activator.CreateInstance(docType) as DocumentBase;
            instance.OpenDocument(fullPath);
            this.DocumentsDisplayed.Add(instance);
            instance.IsSelected = true;
        }

        private void Open()
        {
            var solutionFile = Directory.Exists(this.WorkingDir) ? Directory.EnumerateFiles(this.WorkingDir, "*.assln").FirstOrDefault() : string.Empty;
            if (!string.IsNullOrWhiteSpace(solutionFile))
            {
                try
                {
                    this.CurrentSolution = solutionFile.XmlDeserialize<SolutionUtil.Solution>();
                    this.CurrentSolution.RestoreFromXml(this);
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                        ex = ex.InnerException;
                    //ToDo: Properly write down message
                    MessageBox.Show(ex.Message);
                    this.CurrentSolution = new SolutionUtil.Solution(this);
                }
            }
            else
            {
                //Create new solution as no existing is present
                this.CurrentSolution = new SolutionUtil.Solution(this);
            }
            this.AllPanels.Add(this.CurrentSolution);

            foreach (var panel in this.AllPanels)
            {
                var iniName = panel.GetIniSectionName();
                if (ConfigHost.Instance.LayoutIni.Sections.ContainsSection(iniName))
                {
                    var section = ConfigHost.Instance.LayoutIni[iniName];
                    if (section.ContainsKey("ContentId"))
                    {
                        panel.ContentId = section["ContentId"];
                    }
                    if (section.ContainsKey("IsSelected"))
                    {
                        panel.IsSelected = bool.Parse(section["IsSelected"]);
                    }
                }
            }
        }

        private void Close()
        {
            var solutionFile = Directory.EnumerateFiles(this.WorkingDir, "*.assln").FirstOrDefault();
            this.CurrentSolution.XmlSerialize(Path.Combine(this.WorkingDir, solutionFile == null ? string.Concat(Path.GetFileName(this.WorkingDir), ".assln") : solutionFile));
            //Save Layout GUIDs of the panels
            foreach (var panel in AllPanels)
            {
                var iniName = panel.GetIniSectionName();
                if (!ConfigHost.Instance.LayoutIni.Sections.ContainsSection(iniName))
                    ConfigHost.Instance.LayoutIni.Sections.AddSection(iniName);
                var section = ConfigHost.Instance.LayoutIni[iniName];
                section["ContentId"] = panel.ContentId;
                section["IsSelected"] = panel.IsSelected.ToString();
            }
        }

        private static IEnumerable<PanelBase> FindAllAnchorablePanelsInAssembly()
        {
            var list = new List<PanelBase>();
            foreach (var t in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (!t.IsEquivalentTo(typeof(PanelBase)) && typeof(PanelBase).IsAssignableFrom(t))
                {
                    var instance = Activator.CreateInstance(t, true) as PanelBase;
                    if (instance.AutoAddPanel)
                        list.Add(instance);
                }
            }
            return list;
        }
        private static IEnumerable<DocumentBase> FindAllDocumentsInAssembly()
        {
            var list = new List<DocumentBase>();
            foreach (var t in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                if (!t.IsEquivalentTo(typeof(DocumentBase)) && typeof(DocumentBase).IsAssignableFrom(t))
                {
                    var instance = Activator.CreateInstance(t) as DocumentBase;
                    list.Add(instance);
                }
            }
            return list;
        }
    }
}