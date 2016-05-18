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
        [XmlArray("files")]
        [XmlArrayItem("file")]
        public List<AddInUtil.File> Files { get; set; }


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
            for(int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                file.Parent = this;
                progress.Report(i / Files.Count);
                file.Initialize();
            }
        }
    }

}
