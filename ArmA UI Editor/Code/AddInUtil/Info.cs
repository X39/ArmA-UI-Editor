using System;
using System.Xml.Serialization;
using System.Xml;


namespace ArmA_UI_Editor.Code.AddInUtil
{
    [Serializable]
    public class Info
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
        [XmlElement("author")]
        public string Author { get; set; }
        [XmlElement("version")]
        public string Version { get; set; }
        [XmlElement("updateurl")]
        public string UpdateUrl { get; set; }

        public Info()
        {
            this.Name = default(string);
            this.Description = default(string);
            this.Author = default(string);
            this.Version = default(string);
            this.UpdateUrl = default(string);
        }
    }
}
