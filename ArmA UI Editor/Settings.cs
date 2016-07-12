using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Windows.Markup;

namespace ArmA_UI_Editor
{

    public class HasIgnoreUpdate : MarkupExtension
    {
        public string Path { get; set; }
        public static string CurrentPath { get; set; }

        public HasIgnoreUpdate() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var version = Settings.Instance.IgnoreUpdate;
            var curVersion = Code.UpdateManager.Instance.AppVersion;
            
            return Settings.Instance.IgnoreUpdate != null && (version.Major >= curVersion.Major && version.Minor >= curVersion.Minor && version.Build >= curVersion.Build && version.Minor >= curVersion.Minor) && !version.Equals(curVersion);
        }
    }

    [XmlRoot("settings")]
    public class Settings : IXmlSerializable
    {
        private static Settings _Instance = null;
        public static Settings Instance { get { if (_Instance == null) _Instance = Deserialize(); return _Instance; } }
        #region Serialization
        public static Settings Deserialize()
        {
            try
            {
                var x = new XmlSerializer(typeof(Settings));
                using (var stream = new System.IO.StreamReader(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArmA-UI-Editor", "Settings.xml")))
                {
                    var obj = (Settings)x.Deserialize(stream);
                    return obj;
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return new Settings();
            }
            catch (System.IO.FileNotFoundException)
            {
                return new Settings();
            }
            catch (System.InvalidOperationException ex)
            {
                return new Settings();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + "\n\nSettings got wiped.", "Settings parse error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new Settings();
            }
        }
        public void Save()
        {
            if (preventSave)
                return;
            var x = new XmlSerializer(typeof(Settings));
            if(!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArmA-UI-Editor")))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArmA-UI-Editor"));
            }
            using (var stream = new System.IO.StreamWriter(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ArmA-UI-Editor", "Settings.xml")))
            {
                x.Serialize(stream, this);
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        private bool preventSave = false;
        public void ReadXml(XmlReader reader)
        {
            preventSave = true;

            UsedStyle = null;
            do
            {
                reader.Read();
                switch (reader.Name.ToLower())
                {
                    case "used-style":
                        {
                            reader.Read();
                            var stylePath = reader.Value.ToUpper().Split('/');
                            if (stylePath.Count() <= 1)
                                break;
                            foreach (var addin in Code.AddInManager.Instance.AddIns)
                            {
                                if (addin.Info.Name.ToUpper() == stylePath[0])
                                {
                                    foreach (var style in addin.Styles)
                                    {
                                        if (style.Name.ToUpper() == stylePath[1])
                                        {
                                            UsedStyle.LoadStyle();
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    case "auto-report-crash":
                        {
                            reader.Read();
                            this.AutoReportCrash = bool.Parse(reader.Value);
                        }
                        break;
                    case "search-update-on-start":
                        {
                            reader.Read();
                            this.SearchUpdateOnStart = bool.Parse(reader.Value);
                        }
                        break;
                    case "ignore-update":
                        {
                            reader.Read();
                            this.IgnoreUpdate = System.Version.Parse(reader.Value);
                        }
                        break;
                }
                reader.Read();
            } while (reader.Name.ToUpper() != "SETTINGS" && reader.Name != "");
            preventSave = false;
        }

        public void WriteXml(XmlWriter writer)
        {
            if(this.UsedStyle != null)
            {
                writer.WriteStartElement("used-style");
                writer.WriteString(this.UsedStyle.Parent.Info.Name + '/' + this.UsedStyle.Name);
                writer.WriteEndElement();
                writer.WriteStartElement("auto-report-crash");
                writer.WriteString(this.AutoReportCrash.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("search-update-on-start");
                writer.WriteString(this.SearchUpdateOnStart.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("ignore-update");
                writer.WriteString(this.IgnoreUpdate.ToString());
                writer.WriteEndElement();
            }
        }
        #endregion

        private Code.AddInUtil.Style _UsedStyle;
        public Code.AddInUtil.Style UsedStyle { get { return _UsedStyle; } set { this._UsedStyle = value; Save(); } }
        private bool _AutoReportCrash;
        public bool AutoReportCrash { get { return _AutoReportCrash; } set { this._AutoReportCrash = value; Save(); } }
        private bool _SearchUpdateOnStart;
        public bool SearchUpdateOnStart { get { return _SearchUpdateOnStart; } set { this._SearchUpdateOnStart = value; Save(); } }
        private System.Version _IgnoreUpdate;
        public System.Version IgnoreUpdate { get { return _IgnoreUpdate; } set { this._IgnoreUpdate = value; Save(); } }
        private Settings()
        {
            _UsedStyle = null;
            _AutoReportCrash = true;
            _SearchUpdateOnStart = true;
            _IgnoreUpdate = Code.UpdateManager.Instance.AppVersion;
        }
    }
}
