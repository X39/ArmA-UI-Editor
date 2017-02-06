using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.IO;

namespace ArmA.Studio
{
    public sealed class ConfigHost
    {
        public static ConfigHost Instance { get; private set; }
        static ConfigHost()
        {
            Instance = new ConfigHost();
            
        }

        public IniData LayoutIni { get; private set; }
        public IniData AppIni { get; private set; }


        public ConfigHost()
        {
            string fPath;
            fPath = Path.Combine(App.ConfigPath, "Layout.ini");
            if (File.Exists(fPath))
            {
                var parser = new FileIniDataParser();
                this.LayoutIni = parser.ReadFile(fPath);
            }
            else
            {
                this.LayoutIni = new IniData();
            }
            fPath = Path.Combine(App.ConfigPath, "App.ini");
            if (File.Exists(fPath))
            {
                var parser = new FileIniDataParser();
                this.AppIni = parser.ReadFile(fPath);
            }
            else
            {
                this.AppIni = new IniData();
            }
        }
        public void Save()
        {
            string fPath;
            if (!Directory.Exists(App.ConfigPath))
            {
                Directory.CreateDirectory(App.ConfigPath);
            }
            var parser = new FileIniDataParser();
            fPath = Path.Combine(App.ConfigPath, "Layout.ini");
            parser.WriteFile(fPath, this.LayoutIni);
            fPath = Path.Combine(App.ConfigPath, "App.ini");
            parser.WriteFile(fPath, this.AppIni);
        }

    }
}
