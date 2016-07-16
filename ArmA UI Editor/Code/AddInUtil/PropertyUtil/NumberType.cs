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
    public class NumberType : PType
    {
        public NumberType()
        {
            this.Conversion = string.Empty;
        }
        private EditingSnap Window;

        [XmlAttribute("conversion")]
        public string Conversion { get; set; }

        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var tb = new TextBox();
            tb.Tag = tag;
            this.Window = window;
            var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            if (string.IsNullOrWhiteSpace(Conversion))
                Conversion = string.Empty;
            if (curVal != null)
            {
                switch (Conversion.ToUpper())
                {
                    default:
                        if (tag.PropertyObject is SqfProperty)
                        {
                            tb.Text = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, curVal.String, (int)tag.Extra);
                        }
                        else
                        {
                            tb.Text = curVal.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                        break;
                    case "SCREENX":
                        if (curVal.IsNumber)
                            tb.Text = curVal.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        else
                            tb.Text = window.FromSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.XField, curVal.String).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "SCREENY":
                        if (curVal.IsNumber)
                            tb.Text = curVal.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        else
                            tb.Text = window.FromSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.YField, curVal.String).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "SCREENW":
                        if (curVal.IsNumber)
                            tb.Text = curVal.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        else
                            tb.Text = window.FromSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.WField, curVal.String).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case "SCREENH":
                        if (curVal.IsNumber)
                            tb.Text = curVal.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        else
                            tb.Text = window.FromSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.HField, curVal.String).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        break;
                }
            }
            tb.PreviewTextInput += Tb_PreviewTextInput;
            tb.ToolTip = App.Current.Resources["STR_CODE_Property_Number"] as String;
            return tb;
        }

        private void Tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Code.Utility.tb_PreviewTextInput_Numeric_DoHandle(sender, e, () =>
            {
                var tb = sender as TextBox;
                PTypeDataTag tag = (PTypeDataTag)tb.Tag;
                try
                {
                    switch (Conversion.ToUpper())
                    {
                        default:
                            if (tag.PropertyObject is SqfProperty)
                            {
                                var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
                                AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, field == null && !field.IsString ? "" : field.String, tb.Text, (int)tag.Extra));
                            }
                            else
                            {
                                AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            break;
                        case "SCREENX":
                            AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.XField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture)));
                            break;
                        case "SCREENY":
                            AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.YField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture)));
                            break;
                        case "SCREENW":
                            AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.WField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture)));
                            break;
                        case "SCREENH":
                            AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.HField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture)));
                            break;
                    }
                    TriggerValueChanged(tb);
                }
                catch (Exception ex)
                {
                    TriggerError(tb, ex.Message);
                }
            });
        }
    }
}