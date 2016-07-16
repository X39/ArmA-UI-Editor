using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using ArmA_UI_Editor.UI.Snaps;

namespace ArmA_UI_Editor.Code.AddInUtil.PropertyUtil
{
    public class ArrayType : PType
    {
        [XmlElement("type")]
        public string Type { get; set; }
        [XmlElement("count")]
        public int Count { get; set; }

        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var tb = new TextBox();
            tb.Tag = tag;
            StringBuilder builder = new StringBuilder();
            bool isFirst = true;
            var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            if (curVal != null)
            {
                foreach (var it in curVal.Array)
                {
                    if (!isFirst)
                        builder.Append(", ");
                    isFirst = false;
                    builder.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", it));
                    switch (Type.ToUpper())
                    {
                        case "NUMBER":
                            if (!(it is double))
                                throw new Exception();
                            break;
                        case "STRING":
                            if (!(it is string))
                                throw new Exception();
                            break;
                        case "BOOLEAN":
                            if (!(it is bool))
                                throw new Exception();
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
            tb.Text = builder.ToString();

            tb.PreviewTextInput += Tb_PreviewTextInput;
            switch (Type.ToUpper())
            {
                case "NUMBER":
                    tb.ToolTip = string.Format(System.Globalization.CultureInfo.InvariantCulture, App.Current.Resources["STR_CODE_Property_Array"] as String, App.Current.Resources["STR_CODE_Property_Number"] as String, this.Count);
                    break;
                case "STRING":
                    tb.ToolTip = string.Format(System.Globalization.CultureInfo.InvariantCulture, App.Current.Resources["STR_CODE_Property_Array"] as String, App.Current.Resources["STR_CODE_Property_String"] as String, this.Count);
                    break;
                case "BOOLEAN":
                    tb.ToolTip = string.Format(System.Globalization.CultureInfo.InvariantCulture, App.Current.Resources["STR_CODE_Property_Array"] as String, App.Current.Resources["STR_CODE_Property_Boolean"] as String, this.Count);
                    break;
                default:
                    throw new Exception();
            }
            return tb;
        }

        private void Tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Contains('\r'))
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    var writer = new System.IO.StreamWriter(memStream);
                    writer.Write("class myClass{arr[]={");
                    writer.Write((sender as TextBox).Text);
                    writer.Write("};};");
                    writer.Flush();
                    memStream.Seek(0, System.IO.SeekOrigin.Begin);
                    var mainWindow = ArmA_UI_Editor.UI.MainWindow.TryGet();
                    try
                    {
                        SQF.ClassParser.Generated.Parser p = new SQF.ClassParser.Generated.Parser(new SQF.ClassParser.Generated.Scanner(memStream));
                        var tmpField = p.Parse();
                        if (tmpField.Array.Length != this.Count)
                            throw new Exception();

                        PTypeDataTag tag = (PTypeDataTag)(sender as TextBox).Tag;
                        var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.CreateNew);
                        field.Parent.SetKey(string.Concat(tag.Key, tag.Path), tmpField.Value);
                        TriggerValueChanged(sender);
                    }
                    catch
                    {
                        TriggerError(sender, "Invalid Property");
                    }
                }
            }
        }
    }
}