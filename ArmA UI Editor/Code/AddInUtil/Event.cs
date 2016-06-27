using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    [Serializable]
    public struct Event
    {
        [XmlRoot("root")]
        public class EventList
        {
            [XmlElement("event")]
            public List<Event> List;

            private EventList()
            {
                this.List = new List<Event>();
            }
        }
        [Serializable]
        public struct Argument
        {
            [XmlAttribute("index")]
            public int Index;
            [XmlAttribute("type")]
            public string Type;
            [XmlText]
            public string Description;
        }
        [XmlElement("name")]
        public string Name;
        [XmlElement("field")]
        public string Field;
        [XmlElement("description")]
        public string Description;
        [XmlArray("arguments")]
        [XmlArrayItem("arg")]
        public List<Argument> Arguments;
        [XmlElement("startingAt")]
        public string StartingAt;

        public static List<Event> Load(string path)
        {
            var x = new XmlSerializer(typeof(EventList));
            using (var reader = new System.IO.StreamReader(path))
            {
                var result = (EventList)x.Deserialize(reader);
                foreach(var e in result.List)
                {
                    e.Arguments.Sort((arg1, arg2) => arg1.Index.CompareTo(arg2.Index));
                }
                return result.List;
            }
        }
    }
}