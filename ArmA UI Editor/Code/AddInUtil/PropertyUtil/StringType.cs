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
    public class StringType : PType
    {
        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var tb = new TextBox();
            tb.Tag = tag;
            var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            if (curVal != null)
            {
                if (tag.PropertyObject is SqfProperty)
                {
                    if (curVal != null)
                        tb.Text = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, curVal.String, (int)tag.Extra);
                }
                else
                {
                    tb.Text = curVal.String;
                }
            }
            tb.PreviewTextInput += Tb_PreviewTextInput;
            tb.ToolTip = App.Current.Resources["STR_CODE_Property_String"] as String;
            return tb;
        }
        private void Tb_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Contains('\r'))
            {
                var tb = sender as TextBox;
                PTypeDataTag tag = (PTypeDataTag)tb.Tag;
                object value;
                if (tag.PropertyObject is SqfProperty)
                {
                    var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
                    value = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, field == null && !field.IsString ? "" : field.String, tb.Text, (int)tag.Extra);
                }
                else
                {
                    value = tb.Text;
                }
                AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), value);
                TriggerValueChanged(tb);
            }
        }
    }
}