using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

/*
 * This is pretty much bullshit dump code ...
 * Needs to be rewritten as soon as i start caring about it
 */

namespace SQF.ClassParser
{
    public class File : ConfigClass, IEditableObject
    {
        private ConfigClass dict;
        private ConfigClass backup;

        internal File()
        {
            dict = new ConfigClass();
            backup = null;
        }
        public static File Load(string filePath)
        {
            Parser parser = new Parser(new Scanner(filePath));
            parser.Parse();
            if (parser.errors.count > 0)
                throw new Exception(string.Format("Found {0} errors during parsing", parser.errors.count));
            return parser.Base;
        }
        public void AppendConfig(string filePath)
        {
            Parser parser = new Parser(new Scanner(filePath));
            parser.Base = this;
            parser.Parse();
            if (parser.errors.count > 0)
                throw new Exception(string.Format("Found {0} errors during parsing", parser.errors.count));
        }

        public void BeginEdit()
        {
            backup = dict;
            dict = new ConfigClass(backup);
        }

        public void CancelEdit()
        {
            if (backup == null)
            {
                dict = new ConfigClass();
            }
            else
            {
                dict = backup;
            }
        }

        public void EndEdit()
        {
            if (backup != null)
            {
                merge(backup, dict);
                dict = backup;
                backup = null;
            }
        }

        //TODO: replace Dictionary parameters with ConfigClass and take care that parent classes are also checked (only if they remain the same)
        private void merge(Dictionary<string, Data> origin, Dictionary<string, Data> toMerge)
        {
            foreach(var pair in toMerge)
            {
                if (pair.Value.IsClass)
                {
                    if(origin.ContainsKey(pair.Key))
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
        new public Data this[string key]
        {
            get
            {
                var keys = key.Split('/');
                var tmpDict = dict;
                Data data = default(Data);
                foreach (var it in keys)
                {
                    if (string.IsNullOrWhiteSpace(it))
                        continue;
                    if(dict.ContainsKey(key))
                    {
                        data = dict[key];
                        var val = data.Value;
                        if(val is ConfigClass)
                        {
                            tmpDict = val as ConfigClass;
                        }
                        else if(val is Data)
                        {
                            return val as Data;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else if(backup != null && backup.ContainsKey(key))
                    {
                        data = backup[key];
                        var val = data.Value;
                        if (val is ConfigClass)
                        {
                            tmpDict = val as ConfigClass;
                        }
                        else if (val is Data)
                        {
                            return val as Data;
                        }
                        else
                        {
                            throw new Exception();
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
                var keys = key.Split('/');
                var tmpDict = dict;
                var tmpBackDict = backup;
                foreach (var it in keys)
                {
                    if (string.IsNullOrWhiteSpace(it))
                        continue;
                    if (it == keys.Last())
                    {
                        if (tmpBackDict == null || !tmpBackDict.ContainsKey(it) || !tmpBackDict[it].Equals(tmpDict[it]))
                        {
                            tmpDict[it] = value;
                        }
                    }
                    else if (dict.ContainsKey(it))
                    {
                        tmpDict = dict[it].Value as ConfigClass;
                        if (tmpBackDict != null && tmpBackDict.ContainsKey(it))
                            tmpBackDict = tmpBackDict[it].Value as ConfigClass;
                    }
                    else
                    {
                        dict.Add(it, new Data(new ConfigClass()));
                        if (tmpBackDict != null && tmpBackDict.ContainsKey(it))
                            tmpBackDict = tmpBackDict[it].Value as ConfigClass;
                        else
                            tmpBackDict = null;
                    }
                }
            }
        }
    }
}
