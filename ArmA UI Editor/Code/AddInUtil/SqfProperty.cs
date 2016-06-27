using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Text.RegularExpressions;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    [Serializable]
    public class SqfProperty
    {
        [Serializable]
        public struct Argument
        {
            [XmlAttribute("index")]
            public int Index;
            [XmlElement(ElementName = "number", Type = typeof(Property.NumberType))]
            [XmlElement(ElementName = "string", Type = typeof(Property.StringType))]
            [XmlElement(ElementName = "boolean", Type = typeof(Property.BooleanType))]
            [XmlElement(ElementName = "array", Type = typeof(Property.ArrayType))]
            [XmlElement(ElementName = "listbox", Type = typeof(Property.ListboxType))]
            public Property.PType Property;
        }
        [XmlAttribute("infinite")]
        public bool AllowInfinite;
        [XmlElement("format")]
        public string Format;
        [XmlElement("name")]
        public string Name;
        [XmlElement("arg")]
        public List<Argument> Arguments;
        [XmlIgnore]
        public string Ident { get { return SQF.ClassParser.Data.ToSqfString(this.Format, false); } }

        public static bool HasSqfPropertySection(SqfProperty prop, string fullString)
        {
            return Regex.IsMatch(fullString, string.Format("comment \"<{0}>\";(.*)comment \"</{0}>\";", Regex.Escape(prop.Ident)));
        }
        public static string GetSqfPropertySection(SqfProperty prop, string fullString)
        {
            if (prop.AllowInfinite)
                throw new ArgumentException("This function cannot handle infinite arguments");
            var match = Regex.Match(fullString, string.Format("comment \"<{0}>\";(.*)comment \"</{0}>\";", Regex.Escape(prop.Ident)));
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "";
        }
        public static string GetSqfPropertySectionArg(SqfProperty prop, string fullString, int argIndex)
        {
            var section = GetSqfPropertySection(prop, fullString);
            if (string.IsNullOrWhiteSpace(section))
                return "";
            var filterString = Regex.Replace(prop.Format, @"\{[0-9]*\}", "(.*)");
            var matches = Regex.Split(section, filterString);
            return matches[argIndex + 1];
        }
        public static string SetSqfPropertySectionArg(SqfProperty prop, string fullString, string content, int argIndex)
        {
            if (prop.AllowInfinite)
                throw new ArgumentException("This function cannot handle infinite arguments");
            if(!HasSqfPropertySection(prop, fullString))
            {
                var stringList = new List<string>();
                foreach (var it in prop.Arguments)
                {
                    if (it.Property == null)
                    {
                        stringList.Add("(_this select 0)");
                    }
                    else
                    {
                        stringList.Add("");
                    }
                }
                stringList[argIndex] = content;
                var builder = new StringBuilder();
                builder.Append("comment \"<");
                builder.Append(prop.Ident);
                builder.Append(">\";");
                builder.Append(string.Format(prop.Format, stringList.ToArray()));
                builder.Append("comment \"</");
                builder.Append(prop.Ident);
                builder.Append(">\";");
                return fullString.Insert(0, builder.ToString());
            }
            else
            {
                var section = GetSqfPropertySection(prop, fullString);
                var filterString = Regex.Replace(prop.Format, @"\{[0-9]*\}", "(.*)");
                var matches = Regex.Split(section, filterString);
                matches[argIndex + 1] = content;
                var builder = new StringBuilder();
                builder.Append("comment \"<");
                builder.Append(prop.Ident);
                builder.Append(">\";");
                string[] sArr = new string[matches.Length - 1];
                Array.Copy(matches, 1, sArr, 0, matches.Length - 1);
                builder.Append(string.Format(prop.Format, sArr));
                builder.Append("comment \"</");
                builder.Append(prop.Ident);
                builder.Append(">\";");

                return Regex.Replace(fullString, string.Format("comment \"<{0}>\";.*comment \"</{0}>\";", Regex.Escape(prop.Ident)), builder.ToString());
            }
        }
    }
}
