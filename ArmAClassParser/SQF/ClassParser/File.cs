using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

/*
 * This is pretty much bullshit dump code ...
 * Needs to be rewritten as soon as i start caring about it
 */

namespace SQF.ClassParser
{
    public class File : ConfigClass
    {
        public static File Load(string filePath)
        {
            List<string> errors = new List<string>();
            Parser parser = new Parser(new Scanner(filePath), (s) => { errors.Add(s); });
            parser.Parse();
            if (parser.errors.count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Found {0} errors during parsing {1}\n\n", parser.errors.count, filePath));
                foreach (var s in errors)
                {
                    sb.AppendLine(s);
                }
                throw new Exception(sb.ToString());
            }
            return parser.Base;
        }

        public static File Load(Stream stream)
        {
            List<string> errors = new List<string>();
            Parser parser = new Parser(new Scanner(stream), (s) => { errors.Add(s); });
            parser.Parse();
            if (parser.errors.count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Found {0} errors during parsing from stream\n\n", parser.errors.count));
                foreach (var s in errors)
                {
                    sb.AppendLine(s);
                }
                throw new Exception(sb.ToString());
            }
            return parser.Base;
        }
        public void AppendConfig(Stream stream)
        {
            List<string> errors = new List<string>();
            Parser parser = new Parser(new Scanner(stream), (s) => { errors.Add(s); });
            parser.Base = this;
            parser.Parse();
            if (parser.errors.count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Found {0} errors during parsing from stream\n\n", parser.errors.count));
                foreach (var s in errors)
                {
                    sb.AppendLine(s);
                }
                throw new Exception(sb.ToString());
            }
        }
        public void AppendConfig(string filePath)
        {
            List<string> errors = new List<string>();
            Parser parser = new Parser(new Scanner(filePath), (s) => { errors.Add(s); });
            parser.Base = this;
            parser.Parse();
            if (parser.errors.count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("Found {0} errors during parsing\n\n", parser.errors.count));
                foreach (var s in errors)
                {
                    sb.AppendLine(s);
                }
                throw new Exception(sb.ToString());
            }
        }
        public void AppendConfig(File config)
        {
            this.merge(this, config);
        }

        //TODO: replace Dictionary parameters with ConfigClass and take care that parent classes are also checked (only if they remain the same)
        private void merge(Dictionary<string, Data> origin, Dictionary<string, Data> toMerge)
        {
            foreach (var pair in toMerge)
            {
                if (pair.Value.IsClass)
                {
                    if (origin.ContainsKey(pair.Key))
                    {
                        merge(origin[pair.Key].Class, pair.Value.Class);
                    }
                    else
                    {
                        origin.Add(pair.Key, pair.Value);
                    }
                }
                else
                {
                    if (!origin.ContainsKey(pair.Key) || !origin[pair.Key].Equals(pair.Value))
                    {
                        if (pair.Value == null)
                        {
                            origin.Remove(pair.Key);
                        }
                        else
                        {
                            origin[pair.Key] = pair.Value;
                        }
                    }
                }
            }
        }

        new public Data this[Data d]
        {
            get { if (d == null) return null; return this[d.Name]; }
            set { if (d != null) this[d.Name] = value; }
        }
        public Data this[int i]
        {
            get { return this.ElementAt(i).Value; }
        }
        public Data this[string key]
        {
            get
            {
                var keys = key.Split('/').Where((s) => !string.IsNullOrWhiteSpace(s));
                return this[keys];
            }
            set
            {
                var keys = key.Split('/').Where((s) => !string.IsNullOrWhiteSpace(s));
                this[keys] = value;
            }
        }
        new public Data this[IEnumerable<string> keys]
        {
            get
            {
                ConfigClass tmpDict = this;
                Data data = default(Data);
                foreach (var it in keys)
                {
                    if (string.IsNullOrWhiteSpace(it))
                        continue;
                    if (tmpDict.ContainsKey(it))
                    {
                        data = tmpDict[it];
                        if (data.IsClass)
                        {
                            tmpDict = data.Class;
                        }
                        else
                        {
                            return data;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return data;
            }
            set
            {
                ConfigClass tmpDict = this;
                foreach (var it in keys)
                {
                    if (string.IsNullOrWhiteSpace(it))
                        continue;
                    if (tmpDict.ContainsKey(it))
                    {
                        tmpDict = tmpDict[it].Value as ConfigClass;
                    }
                    else
                    {
                        tmpDict.Add(it, value);
                    }
                }
            }
        }

        public Data ReceiveField(string path, string field, Data start = null)
        {
            var data = start != null ? start : this[path + field];
            
            if (data == null)
            {
                var tmpData = this[path];
                if (tmpData != null && tmpData.IsClass && tmpData.Class.Parent != null)
                {
                    do
                    {
                        path = path.Substring(0, path.LastIndexOf('/'));
                        data = this[path + '/' + tmpData.Class.Parent.Name + field];
                    } while (path.Length > 0 && data == null);
                }
            }
            return data;
        }
    }
}
