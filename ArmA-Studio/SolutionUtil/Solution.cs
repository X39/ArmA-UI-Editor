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
using Utility.Collections;

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
        public ObservableSortedCollection<SolutionFileBase> FilesCollection { get { return this._FilesCollection; } set { this._FilesCollection = value; this.RaisePropertyChanged(); } }
        private ObservableSortedCollection<SolutionFileBase> _FilesCollection;

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

        private Workspace curWorkspace;
        
        public Solution()
        {
        }
        public void Prepare(Workspace workspace)
        {
            if (this.curWorkspace == workspace)
                return;
            if(this._FilesCollection == null)
                this._FilesCollection = new ObservableSortedCollection<SolutionFileBase>();
            this.CmdContextMenu_Add_NewItem = new UI.Commands.RelayCommand(this.CreateNewItem);
            this.curWorkspace = workspace;
            this.FSWatcher = new FileSystemWatcher(this.curWorkspace.WorkingDir);
            this.FSWatcher.Changed += FSWatcher_Changed;
        }
        public void ReScan()
        {
            foreach (var file in Directory.EnumerateFiles(this.curWorkspace.WorkingDir, "*.*", SearchOption.AllDirectories).Pick((it) => FileFilter.Contains(Path.GetExtension(it))))
            {
                this.GetOrCreateFileReference(file);
            }
        }
        public void CreateNewItem(object param)
        {
            var path = param as string;
            if (path == null)
                return;
            SolutionFileBase sfb;
            var result = ShowCreateFileDialog(path, out sfb);
            if(result)
            {
                this.curWorkspace.OpenOrFocusDocument(sfb.FullPath);
            }
        }

        /// <summary>
        /// Displays the <see cref="Dialogs.CreateNewFileDialog"/> and creates the file.
        /// </summary>
        /// <param name="fileDirectory">Base directory (relative or full) to use.</param>
        /// <param name="newFile">Will be either null if user aborted or the created <see cref="SolutionFileBase"/>.</param>
        /// <returns><see cref="true"/> if file was created, <see cref="false"/> if not.</returns>
        public bool ShowCreateFileDialog(string fileDirectory, out SolutionFileBase newFile)
        {
            var dlgDc = new Dialogs.CreateNewFileDialogDataContext();
            var dlg = new Dialogs.CreateNewFileDialog(dlgDc);
            fileDirectory = fileDirectory.TrimStart('/', '\\');
            if (!Path.IsPathRooted(fileDirectory))
            {
                fileDirectory = Path.Combine(this.curWorkspace.WorkingDir, fileDirectory);
            }

            var dlgResult = dlg.ShowDialog();
            if(dlgResult.HasValue && dlgResult.Value)
            {
                var fName = dlgDc.SelectedName;
                if(!Path.HasExtension(fName))
                {
                    fName = string.Concat(fName, '.', ((Dialogs.FileType)dlgDc.SelectedItem).Extension);
                }
                using (var writer = File.CreateText(Path.Combine(fileDirectory, fName)))
                {
                    writer.Write(((Dialogs.FileType)dlgDc.SelectedItem).DefaultContent);
                }
                newFile = GetOrCreateFileReference(Path.Combine(fileDirectory, fName));

                //Expand the tree till here
                var current = newFile.Parent;
                while(current != null)
                {
                    current.IsExpanded = true;
                    current = current.Parent;
                }
                //Select new file
                newFile.IsSelected = true;
                return true;
            }
            else
            {
                newFile = null;
                return false;
            }
        }

        private void FSWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            SolutionFile solutionFile = null;
            
            var existsInSolution = SolutionFileBase.WalkThrough(this.FilesCollection, (sfb) =>
            {
                if(sfb is SolutionFile && sfb.FullPath.Equals(sfb.FullPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    solutionFile = sfb as SolutionFile;
                    return true;
                }
                return false;
            });
            if(existsInSolution)
            {
                DocumentBase docBase = null;
                foreach(var it in this.curWorkspace.DocumentsDisplayed)
                {
                    if(it.FilePath == e.FullPath)
                    {
                        docBase = it;
                        break;
                    }
                }
                if (docBase == null)
                    return;
                var msgBoxResult = MessageBox.Show(Properties.Localization.MessageBoxFileChanged_Body, Properties.Localization.MessageBoxFileChanged_Title, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (msgBoxResult != MessageBoxResult.Yes)
                    return;
                docBase.ReloadDocument();
            }
        }

        internal SolutionFileBase GetOrCreateFileReference(string path)
        {
            path = path.TrimStart('/', '\\');
            if (Path.IsPathRooted(path))
            {
                path = path.Substring(this.curWorkspace.WorkingDir.Length + 1);
            }
            ICollection<SolutionFileBase> sfbCollection = null;
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

        internal void RestoreFromXml()
        {
            SolutionFileBase.WalkThrough(this.FilesCollection, (it) =>
            {
                if (it.Children != null)
                {
                    foreach (var child in it.Children)
                        if(child.Parent != it)
                            child.Parent = it;
                }
                return false;
            });
        }
    }
}
