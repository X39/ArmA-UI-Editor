using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ArmA_UI_Editor
{
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
                switch (reader.Name.ToUpper())
                {
                    case "USED-STYLE":
                        reader.Read();
                        var stylePath = reader.Value.ToUpper().Split('/');
                        if (stylePath.Count() <= 1)
                            break;
                        foreach (var addin in Code.AddInManager.Instance.AddIns)
                        {
                            if(addin.Info.Name.ToUpper() == stylePath[0])
                            {
                                foreach(var style in addin.Styles)
                                {
                                    if(style.Name.ToUpper() == stylePath[1])
                                    {
                                        UsedStyle.LoadStyle();
                                        break;
                                    }
                                }
                                break;
                            }
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
            }
        }
        #endregion

        private Code.AddInUtil.Style _UsedStyle;
        public Code.AddInUtil.Style UsedStyle { get { return _UsedStyle; } set { this._UsedStyle = value; Save(); } }
        private Settings()
        {
            UsedStyle = null;
        }
    }
}
