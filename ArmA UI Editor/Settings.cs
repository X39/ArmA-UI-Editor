using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ArmA_UI_Editor
{
    [Serializable]
    [XmlRoot("AddIn")]
    internal class Settings
    {
        private static Settings _Instance = Deserialize();
        public static Settings Instance { get { return _Instance; } }

        private static Settings Deserialize()
        {
            var x = new XmlSerializer(typeof(Settings));
            using (var reader = new System.IO.StreamReader("Settings.xml"))
            {
                var obj = (Settings)x.Deserialize(reader);
                return obj;
            }
        }

    }
}
