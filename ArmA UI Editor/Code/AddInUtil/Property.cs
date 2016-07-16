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

namespace ArmA_UI_Editor.Code.AddInUtil
{
    public class Property
    {
        public class PTypeDataTag
        {
            public string Key;
            public string Path;
            public object PropertyObject;
            public object Extra;
        }
        public abstract class PType
        {
            public abstract FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag);
            public static event EventHandler ValueChanged;
            public static event EventHandler<string> OnError;
            protected void TriggerValueChanged(object sender)
            {
                if (ValueChanged != null)
                    ValueChanged(sender, new EventArgs());
            }
            protected void TriggerError(object sender, string msg)
            {
                if (OnError != null)
                    OnError(sender, msg);
            }
        }
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
                    PTypeDataTag tag = tb.Tag as PTypeDataTag;
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
                    PTypeDataTag tag = tb.Tag as PTypeDataTag;
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
                PTypeDataTag tag = cb.Tag as PTypeDataTag;
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

                            PTypeDataTag tag = (sender as TextBox).Tag as PTypeDataTag;
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
        public class ListboxType : PType
        {
            public class Data
            {
                public Data()
                {
                    this.Name = string.Empty;
                    this.Value = string.Empty;
                    this.Type = string.Empty;
                }
                [XmlAttribute("display")]
                public string Name { get; set; }
                [XmlAttribute("value")]
                public string Value { get; set; }
                [XmlAttribute("as")]
                public string Type { get; set; }
            }

            [XmlArray("items")]
            [XmlArrayItem("item")]
            public List<Data> Items { get; set; }

            public override FrameworkElement GenerateUiElement(string Key, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
            {
                var cb = new ComboBox();
                cb.Tag = tag;
                cb.DisplayMemberPath = "Name";
                cb.SelectedValuePath = "Value";
                var curVal = AddInManager.Instance.MainFile.GetKey(Key, SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
                foreach (var it in this.Items)
                {
                    cb.Items.Add(it);
                    if (curVal != null)
                    {
                        if (tag.PropertyObject is SqfProperty)
                        {
                            var val = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, curVal.String, (int)tag.Extra);
                            if (it.Value == val)
                                cb.SelectedItem = it;
                        }
                        else
                        {
                            if (it.Value == string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", curVal.Value))
                                cb.SelectedItem = it;
                        }
                    }
                    
                        
                }
                cb.SelectionChanged += Cb_SelectionChanged;
                cb.ToolTip = App.Current.Resources["STR_CODE_Property_ValueList"] as String;
                return cb;
            }

            private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ComboBox cb = sender as ComboBox;
                PTypeDataTag tag = cb.Tag as PTypeDataTag;

                Data d = cb.Items[cb.SelectedIndex] as Data;
                object value = null;
                switch (d.Type.ToUpper())
                {
                    default:
                        value = d.Value;
                        break;
                    case "NUMBER":
                        value = double.Parse(d.Value, System.Globalization.CultureInfo.InvariantCulture);
                        break;
                }

                if (tag.PropertyObject is SqfProperty)
                {
                    var field = AddInManager.Instance.MainFile.GetKey(string.Concat(tag.Key, tag.Path), SQF.ClassParser.ConfigField.KeyMode.NullOnNotFound);
                    if(field != null && field.IsString)
                        value = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, string.IsNullOrWhiteSpace(field.String) ? "" : field.String, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", value), (int)tag.Extra);
                }
                AddInManager.Instance.MainFile.SetKey(string.Concat(tag.Key, tag.Path), value);
                TriggerValueChanged(value);
            }
        }

        [XmlElement("field")]
        public string FieldPath { get; set; }
        [XmlElement("name")]
        public string DisplayName { get; set; }
        [XmlElement(ElementName = "number", Type = typeof(NumberType))]
        [XmlElement(ElementName = "string", Type = typeof(StringType))]
        [XmlElement(ElementName = "boolean", Type = typeof(BooleanType))]
        [XmlElement(ElementName = "array", Type = typeof(ArrayType))]
        [XmlElement(ElementName = "listbox", Type = typeof(ListboxType))]
        public PType PropertyType { get; set; }
    }
}
