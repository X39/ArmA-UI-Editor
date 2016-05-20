using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace ArmA_UI_Editor.Code
{
    [Serializable]
    [XmlRoot("AddIn")]
    public class AddIn
    {
        [XmlIgnore()]
        public string ThisPath { get; set; }

        [XmlElement("info")]
        public AddInUtil.Info Info { get; set; }
        [XmlArray("uielements")]
        [XmlArrayItem("element")]
        public List<AddInUtil.UIElement> UIElements { get; set; }
        [XmlArray("styles")]
        [XmlArrayItem("style")]
        public List<AddInUtil.Style> Styles { get; set; }


        public AddIn()
        {
        }

        public static AddIn LoadAddIn(string path)
        {
            var x = new XmlSerializer(typeof(AddIn));
            using (var reader = new System.IO.StreamReader(path))
            {
                var addIn = (AddIn)x.Deserialize(reader);
                addIn.ThisPath = path.Substring(0, path.LastIndexOf('\\'));
                return addIn;
            }
        }

        public void Initialize(IProgress<double> progress)
        {
            for (int i = 0; i < UIElements.Count; i++)
            {
                var file = UIElements[i];
                file.Parent = this;
                progress.Report(i / (UIElements.Count + this.Styles.Count));
                file.Initialize();
            }
            for (int i = 0; i < Styles.Count; i++)
            {
                var style = Styles[i];
                progress.Report(i / (UIElements.Count + this.Styles.Count));
                style.Initialize(this.ThisPath);
            }
        }
    }

}
