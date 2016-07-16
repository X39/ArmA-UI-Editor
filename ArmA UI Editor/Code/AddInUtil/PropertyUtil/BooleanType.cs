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
using ArmA_UI_Editor.Code.AddInUtil.PropertyUtil;

namespace ArmA_UI_Editor.Code.AddInUtil.PropertyUtil
{
    public class BooleanType : PType
    {
        public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
        {
            var cb = new ComboBox();
            cb.Tag = tag;
            cb.Items.Add("true");
            cb.Items.Add("false");
            var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
            if (curVal != null)
            {
                if (tag.PropertyObject is SqfProperty)
                {
                    var str = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, curVal.String, (int)tag.Extra);
                    if (!string.IsNullOrWhiteSpace(str))
                        cb.SelectedIndex = bool.Parse(str) ? 0 : 1;
                }
                else
                {
                    cb.SelectedIndex = curVal.Boolean ? 0 : 1;
                }
            }
            cb.SelectionChanged += Cb_SelectionChanged;
            cb.ToolTip = App.Current.Resources["STR_CODE_Property_Boolean"] as String;
            return cb;
        }

        private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            PTypeDataTag tag = (PTypeDataTag)cb.Tag;
            object value;
            if (tag.PropertyObject is SqfProperty)
            {
                var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
                value = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, field == null && !field.IsString ? "" : field.String, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cb.SelectedValue), (int)tag.Extra);
            }
            else
            {
                value = bool.Parse((string)cb.SelectedValue);
            }
            AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), value);
            TriggerValueChanged(cb);
        }
    }
}