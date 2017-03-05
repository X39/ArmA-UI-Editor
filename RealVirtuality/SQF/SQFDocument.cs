using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RealVirtuality.SQF
{
    public class SQFDocument
    {
        internal SQFDocument() : this(new List<SQFExpression>(), new List<ParseInfo>()) { }
        internal SQFDocument(List<SQFExpression> expressions, List<ParseInfo> infoEntries)
        {
            this.Expressions = expressions;
            this.InfoEntries = infoEntries;
        }

        public List<SQFExpression> Expressions { get; internal set; }
        public List<ParseInfo> InfoEntries { get; internal set; }

        public static SQFDocument CreateNew()
        {
            return new SQFDocument();
        }
        public static SQFDocument ParseDocument(StreamReader reader)
        {
            var definitions = new List<SQFDefinition>();
            int offset = 0;
            int line = 0;
            int curLineOffset = 0;
            int expStart = 0;
            int expStartLine = 0;
            var doc = new SQFDocument();
            ParseHelper.ParseSQFDocument(doc, reader, definitions, ref offset, ref line, ref curLineOffset, ref expStart, ref expStartLine);
            return doc;
        }
    }
}
