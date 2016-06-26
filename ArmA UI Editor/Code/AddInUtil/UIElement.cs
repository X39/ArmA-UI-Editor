using System;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    [Serializable]
    public class UIElement
    {
        [XmlElement("displayname")]
        public string DisplayName { get; set; }
        [XmlElement("image")]
        public string Image { get; set; }
        [XmlElement("class")]
        public string __ClassPath { get; set; }
        [XmlElement("xaml")]
        public string __XamlPath { get; set; }
        [XmlElement("properties")]
        public List<string> __PropertiesPath { get; set; }
        [XmlElement("events")]
        public List<string> __EventsPath { get; set; }

        [XmlIgnore()]
        public SQF.ClassParser.File ClassFile { get; set; }
        [XmlIgnore()]
        public AddIn Parent { get; set; }
        [XmlIgnore()]
        public List<Group> Properties { get; private set; }
        [XmlIgnore()]
        public List<Event> Events { get; private set; }

        public UIElement()
        {
            this.DisplayName = default(string);
            this.Image = default(string);
            this.__ClassPath = default(string);
            this.__XamlPath = default(string);
            this.__PropertiesPath = default(List<string>);
            this.__EventsPath = default(List<string>);
            this.Properties = new List<Group>();
            this.Events = new List<Event>();
        }
        
        public void Initialize()
        {
            this.Image = Parent.ThisPath + Image.Replace('/', '\\');
            this.__ClassPath = Parent.ThisPath + __ClassPath.Replace('/', '\\');
            this.__XamlPath = Parent.ThisPath + __XamlPath.Replace('/', '\\');
            this.ClassFile = SQF.ClassParser.File.Load(this.__ClassPath);

            foreach (var it in this.__PropertiesPath)
            {
                var path = System.IO.Path.Combine(Parent.ThisPath, it.TrimStart(new[] { '\\', '/' }));
                var groups = Group.Load(path);
                foreach(var groupA in this.Properties)
                {
                    foreach(var groupB in groups)
                    {
                        if(groupA.Name.ToUpper() == groupB.Name.ToUpper())
                        {
                            groups.Remove(groupB);
                            groupA.Items.AddRange(groupB.Items);
                            break;
                        }
                    }
                }
                this.Properties.AddRange(groups);
            }
            foreach (var it in this.__EventsPath)
            {
                var path = System.IO.Path.Combine(Parent.ThisPath, it.TrimStart(new[] { '\\', '/' }));
                this.Events.AddRange(Event.Load(path));
            }
        }
    }
}