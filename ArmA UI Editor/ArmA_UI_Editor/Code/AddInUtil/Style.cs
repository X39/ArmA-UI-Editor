using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Media;
using System.Windows.Media;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    public class Style
    {
        [XmlAttribute("path")]
        public string Path;
        [XmlAttribute("name")]
        public string Name;
        [XmlIgnore]
        public System.Windows.ResourceDictionary Resources;
        [XmlIgnore]
        public AddIn Parent;

        public Style() { }
        public void Initialize(string basePath)
        {
            this.Path = basePath + this.Path.Replace('/', '\\');
            using (var stream = System.IO.File.OpenRead(this.Path))
            {
                this.Resources = (System.Windows.ResourceDictionary)System.Windows.Markup.XamlReader.Load(stream);
            }
        }

        public void LoadStyle()
        {
            if(Settings.Instance.UsedStyle != this)
            {
                Settings.Instance.UsedStyle = this;
            }
            return;
            /*
            //RTE due to accessing frozen app context
            foreach(var it in Resources.Keys)
            {
                if(it is string && Resources[it] is SolidColorBrush)
                {
                    string key = (string)it;
                    var res = App.Current.Resources[key];
                    if(res is SolidColorBrush)
                    {
                        App.Current.Resources[key] = Resources[key];
                    }
                }
            }*/
        }
    }
}