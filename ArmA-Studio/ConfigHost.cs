using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.IO;
using System.Windows.Media;
using Utility;

namespace ArmA.Studio
{
    public sealed class ConfigHost
    {
        public enum EIniSelector
        {
            App,
            Coloring,
            Layout
        }
        public static class App
        {
            public static string WorkspacePath { get { return Instance.AppIni.GetValueOrNull("App", "Workspace"); } set { Instance.AppIni.SetValue("App", "Workspace", value); Instance.Save(EIniSelector.App); } }

        }
        public static class Coloring
        {
            public static void Reset()
            {
                SyntaxErrorColor = Color.FromArgb(255, 255, 0, 0);
                HighlightColor = Color.FromArgb(16, 0, 0, 0);
                HighlightColorBorder = Color.FromArgb(32, 0, 0, 0);
                BreakpointColor = Color.FromArgb(255, 255, 0, 0);
                BreakpointColorBorder = Color.FromArgb(255, 255, 255, 255);
                BreakpointRectColor = Color.FromArgb(32, 200, 0, 0);
            }
            public static Color ColorParse(string inputs)
            {
                if (string.IsNullOrWhiteSpace(inputs))
                    return default(Color);
                var splitInputs = inputs.Split(',');
                if (splitInputs.Length != 4)
                    return default(Color);
                foreach (var input in splitInputs)
                {
                    if (!input.IsInteger())
                    {
                        return default(Color);
                    }
                }
                return Color.FromArgb(byte.Parse(splitInputs[0]), byte.Parse(splitInputs[1]), byte.Parse(splitInputs[2]), byte.Parse(splitInputs[3]));
            }
            public static string ColorParse(Color colorInput)
            {
                return string.Join(",", colorInput.A, colorInput.R, colorInput.G, colorInput.B);
            }

            public static Color SyntaxErrorColor
            {
                get { return ColorParse(Instance.ColoringIni.GetValueOrNull("Coloring", "SyntaxErrorColor")); }
                set { Instance.ColoringIni.SetValue("Coloring", "SyntaxErrorColor", ColorParse(value)); Instance.Save(EIniSelector.Coloring); }
            }

            public static Color HighlightColor
            {
                get { return ColorParse(Instance.ColoringIni.GetValueOrNull("Coloring", "HighlightColor")); }
                set { Instance.ColoringIni.SetValue("Coloring", "HighlightColor", ColorParse(value)); Instance.Save(EIniSelector.Coloring); }
            }
            public static Color HighlightColorBorder
            {
                get { return ColorParse(Instance.ColoringIni.GetValueOrNull("Coloring", "HighlightColorBorder")); }
                set { Instance.ColoringIni.SetValue("Coloring", "HighlightColorBorder", ColorParse(value)); Instance.Save(EIniSelector.Coloring); }
            }

            public static Color BreakpointColor
            {
                get { return ColorParse(Instance.ColoringIni.GetValueOrNull("Coloring", "BreakpointColor")); }
                set { Instance.ColoringIni.SetValue("Coloring", "BreakpointColor", ColorParse(value)); Instance.Save(EIniSelector.Coloring); }
            }
            public static Color BreakpointColorBorder
            {
                get { return ColorParse(Instance.ColoringIni.GetValueOrNull("Coloring", "BreakpointColorBorder")); }
                set { Instance.ColoringIni.SetValue("Coloring", "BreakpointColorBorder", ColorParse(value)); Instance.Save(EIniSelector.Coloring); }
            }

            public static Color BreakpointRectColor
            {
                get { return ColorParse(Instance.ColoringIni.GetValueOrNull("Coloring", "BreakpointRectColor")); }
                set { Instance.ColoringIni.SetValue("Coloring", "BreakpointRectColor", ColorParse(value)); Instance.Save(EIniSelector.Coloring); }
            }

        }
        public static ConfigHost Instance { get; private set; }
        static ConfigHost()
        {
            Instance = new ConfigHost();
            if(Instance.ColoringIni.Sections.Count == 0)
            {
                Coloring.Reset();
            }
        }

        public IniData LayoutIni { get; private set; }
        public IniData AppIni { get; private set; }
        public IniData ColoringIni { get; private set; }


        public ConfigHost()
        {
            string fPath;
            fPath = Path.Combine(ArmA.Studio.App.ConfigPath, "Layout.ini");
            if (File.Exists(fPath))
            {
                var parser = new FileIniDataParser();
                this.LayoutIni = parser.ReadFile(fPath);
            }
            else
            {
                this.LayoutIni = new IniData();
            }
            fPath = Path.Combine(ArmA.Studio.App.ConfigPath, "App.ini");
            if (File.Exists(fPath))
            {
                var parser = new FileIniDataParser();
                this.AppIni = parser.ReadFile(fPath);
            }
            else
            {
                this.AppIni = new IniData();
            }
            fPath = Path.Combine(ArmA.Studio.App.ConfigPath, "Coloring.ini");
            if (File.Exists(fPath))
            {
                var parser = new FileIniDataParser();
                this.ColoringIni = parser.ReadFile(fPath);
            }
            else
            {
                this.ColoringIni = new IniData();
            }
        }
        public void SaveAll()
        {
            foreach(EIniSelector sel in Enum.GetValues(typeof(EIniSelector)))
            {
                this.Save(sel);
            }
        }

        public void Save(EIniSelector selector)
        {
            if (!Directory.Exists(ArmA.Studio.App.ConfigPath))
            {
                Directory.CreateDirectory(ArmA.Studio.App.ConfigPath);
            }
            var parser = new FileIniDataParser();
            switch (selector)
            {
                case EIniSelector.App:
                    parser.WriteFile(Path.Combine(ArmA.Studio.App.ConfigPath, "App.ini"), this.AppIni);
                    break;
                case EIniSelector.Coloring:
                    parser.WriteFile(Path.Combine(ArmA.Studio.App.ConfigPath, "Coloring.ini"), this.ColoringIni);
                    break;
                case EIniSelector.Layout:
                    parser.WriteFile(Path.Combine(ArmA.Studio.App.ConfigPath, "Layout.ini"), this.LayoutIni);
                    break;
            }
        }
    }
}
