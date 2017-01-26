using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRealityEngine.Media.Drawing.PAA.TaggUtil;


namespace VirtualRealityEngine.Media.Drawing.PAA
{
    public class PAAImage
    {
        public List<TagBase> Tags { get; private set; }
        
        public PAAImage()
        {
            this.Tags = new List<TagBase>();
        }
    }
}
