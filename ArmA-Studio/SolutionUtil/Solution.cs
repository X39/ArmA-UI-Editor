using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Xml.Serialization;
using ArmA.Studio.UI;
using Utility;

namespace ArmA.Studio.SolutionUtil
{
    [XmlRoot("solution")]
    public sealed class Solution : PanelBase, UI.ViewModel.IPropertyDatatemplateProvider
    {
        public static string[] FileFilter = new string[] { ".sqf", ".cpp", ".ext", ".hpp", ".txt" };
        [XmlArray("files")]
        [XmlArrayItem]
        [XmlArrayItem("folder", Type = typeof(SolutionFolder))]
        [XmlArrayItem("file", Type = typeof(SolutionFile))]
        public ObservableCollection<SolutionFileBase> FilesCollection { get { return this._FilesCollection; } set { this._FilesCollection = value; this.RaisePropertyChanged(); } }

        [XmlIgnore]
        public DataTemplate PropertiesTemplate
        {
            get
            {
                return null;
            }
        }

        [XmlIgnore]
        public override string Title { get { return Properties.Localization.PanelDisplayName_Solution; } }
        [XmlIgnore]
        public override bool AutoAddPanel { get { return false; } }

        [XmlIgnore]
        public ICommand CmdContextMenu_Add_NewItem { get; private set; }
        [XmlIgnore]
        public ICommand CmdContextMenu_Add_ExistingItem { get; private set; }
        [XmlIgnore]
        public ICommand CmdContextMenu_Add_NewFolder { get; private set; }
        [XmlIgnore]
        public ICommand CmdContextMenu_RescanWorkspace { get; private set; }

        [XmlIgnore]
        public FileSystemWatcher FSWatcher { get; private set; }

        private ObservableCollection<SolutionFileBase> _FilesCollection;
        private Workspace curWorkspace;

        private Solution() { }

        public Solution(Workspace workspace)
        {
            this._FilesCollection = new ObservableCollection<SolutionFileBase>();
            this.curWorkspace = workspace;
            this.FSWatcher = new FileSystemWatcher(workspace.WorkingDir);
            foreach(var file in Directory.EnumerateFiles(workspace.WorkingDir, "*.*", SearchOption.AllDirectories).Pick((it) => FileFilter.Contains(Path.GetExtension(it))))
            {
                this.GetOrCreateFileReference(file);
            }
        }
        internal SolutionFileBase GetOrCreateFileReference(string path)
        {
            if(Path.IsPathRooted(path))
            {
                path = path.Substring(this.curWorkspace.WorkingDir.Length + 1);
            }
            Collection<SolutionFileBase> sfbCollection = null;
            SolutionFileBase sfbCollectionOwner = null;
            var parent = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(parent))
            {
                sfbCollection = this.FilesCollection;
            }
            else
            {
                SolutionFileBase.WalkThrough(this.FilesCollection, (it) =>
                {
                    if (it.RelativePath.Equals(parent))
                    {
                        sfbCollectionOwner = it;
                        sfbCollection = it.Children;
                        return true;
                    }
                    return false;
                });
            }
            if (sfbCollection == null)
            {
                sfbCollectionOwner = GetOrCreateFileReference(parent);
                sfbCollection = sfbCollectionOwner.Children;
            }
            foreach (var it in sfbCollection)
            {
                if (it.FileName.Equals(filename))
                {
                    return it;
                }
            }
            SolutionFileBase sfb;
            if((File.GetAttributes(Path.Combine(this.curWorkspace.WorkingDir, path)) & FileAttributes.Directory) > 0)
            {
                sfb = new SolutionFolder() { FileName = filename, Parent = sfbCollectionOwner };
            }
            else
            {
                sfb = new SolutionFile() { FileName = filename, Parent = sfbCollectionOwner };
            }
            sfbCollection.Add(sfb);
            return sfb;
        }

        internal void RestoreFromXml(Workspace workspace)
        {
            this.curWorkspace = workspace;
            SolutionFileBase.WalkThrough(this.FilesCollection, (it) =>
            {
                if (it.Children != null)
                {
                    foreach (var child in it.Children)
                        child.Parent = it;
                }
                return false;
            });
        }
    }
}
