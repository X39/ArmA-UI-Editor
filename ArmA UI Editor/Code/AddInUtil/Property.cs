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
            public SQF.ClassParser.File File;
            public SQF.ClassParser.Data BaseData;
            public string Path;
            public object PropertyObject;
            public object Extra;
        }
        public abstract class PType
        {
            public abstract FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag);
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
            public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
            {
                var tb = new TextBox();
                tb.Tag = tag;
                if (tag.PropertyObject is SqfProperty)
                {
                    if(curVal != null)
                        tb.Text = SqfProperty.GetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, curVal.String, (int)tag.Extra);
                }
                else
                {
                    tb.Text = curVal != null ? curVal.String : "";
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
                    var data = tag.File[tag.Path];
                    if (data == null)
                        data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                    if (tag.PropertyObject is SqfProperty)
                    {
                        data.String = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, string.IsNullOrWhiteSpace(data.String) ? "" : data.String, tb.Text, (int)tag.Extra);
                    }
                    else
                    {
                        data.String = tb.Text;
                    }
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

            public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
            {
                var tb = new TextBox();
                tb.Tag = tag;
                this.Window = window;
                if (curVal != null)
                {
                    if (string.IsNullOrWhiteSpace(Conversion))
                        Conversion = string.Empty;
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
                    var data = tag.File[tag.Path];
                    if (data == null)
                        data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                    try
                    {
                        switch (Conversion.ToUpper())
                        {
                            default:
                                if (tag.PropertyObject is SqfProperty)
                                {
                                    data.String = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, data.Value == null ? "" : data.String, tb.Text, (int)tag.Extra);
                                }
                                else
                                {
                                    data.Number = double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture);
                                }
                                break;
                            case "SCREENX":
                                data.String = Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.XField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case "SCREENY":
                                data.String = Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.YField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case "SCREENW":
                                data.String = Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.WField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture));
                                break;
                            case "SCREENH":
                                data.String = Window.ToSqfString(ArmA_UI_Editor.UI.Snaps.EditingSnap.FieldTypeEnum.HField, double.Parse(tb.Text, System.Globalization.CultureInfo.InvariantCulture));
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
            public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
            {
                var cb = new ComboBox();
                cb.Tag = tag;
                cb.Items.Add("true");
                cb.Items.Add("false");
                if (curVal != null)
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

                cb.SelectionChanged += Cb_SelectionChanged;
                cb.ToolTip = App.Current.Resources["STR_CODE_Property_Boolean"] as String;
                return cb;
            }

            private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ComboBox cb = sender as ComboBox;
                PTypeDataTag tag = cb.Tag as PTypeDataTag;
                var data = tag.File[tag.Path];
                if (data == null)
                    data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                if (tag.PropertyObject is SqfProperty)
                {
                    data.String = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, string.IsNullOrWhiteSpace(data.String) ? "" : data.String, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", cb.SelectedValue), (int)tag.Extra);
                }
                else
                {
                    data.Boolean = bool.Parse((string)cb.SelectedValue);
                }
                TriggerValueChanged(cb);
            }
        }
        public class ArrayType : PType
        {
            [XmlElement("type")]
            public string Type { get; set; }
            [XmlElement("count")]
            public int Count { get; set; }

            public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
            {
                var tb = new TextBox();
                tb.Tag = tag;
                StringBuilder builder = new StringBuilder();
                bool isFirst = true;
                if (curVal != null)
                {
                    foreach (var it in curVal.Array)
                    {
                        if (!isFirst)
                            builder.Append(", ");
                        isFirst = false;
                        builder.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", it.Value));
                        switch (Type.ToUpper())
                        {
                            case "NUMBER":
                                if (!it.IsNumber)
                                    throw new Exception();
                                break;
                            case "STRING":
                                if (!it.IsString)
                                    throw new Exception();
                                break;
                            case "BOOLEAN":
                                if (!it.IsBoolean)
                                    throw new Exception();
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    tb.Text = builder.ToString();
                }
                tb.TextChanged += Tb_TextChanged;
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
                            var file = SQF.ClassParser.File.Load(memStream);
                            if (file["/myClass/arr"].Array.Count != this.Count)
                                throw new Exception();

                            PTypeDataTag tag = (sender as TextBox).Tag as PTypeDataTag;
                            var data = tag.File[tag.Path];
                            if (data == null)
                                data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                            data.Array = file["/myClass/arr"].Array;
                            TriggerValueChanged(sender);
                        }
                        catch
                        {
                            TriggerError(sender, "Invalid Property");
                        }
                    }
                }
            }

            private void Tb_TextChanged(object sender, TextChangedEventArgs e)
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
                        var file = SQF.ClassParser.File.Load(memStream);
                        if (!(sender as TextBox).Text.Contains('\r') || file["/myClass/arr"].Array.Count != this.Count)
                            return;

                        PTypeDataTag tag = (sender as TextBox).Tag as PTypeDataTag;
                        var data = tag.File[tag.Path];
                        if (data == null)
                            data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
                        data.Array = file["/myClass/arr"].Array;
                        TriggerValueChanged(sender);
                    }
                    catch
                    {
                        TriggerError(sender, "Invalid Property");
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

            public override FrameworkElement GenerateUiElement(SQF.ClassParser.Data curVal, ArmA_UI_Editor.UI.Snaps.EditingSnap window, PTypeDataTag tag)
            {
                var cb = new ComboBox();
                cb.Tag = tag;
                cb.DisplayMemberPath = "Name";
                cb.SelectedValuePath = "Value";
                foreach (var it in this.Items)
                {
                    cb.Items.Add(it);
                    if (curVal != null)
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
                cb.SelectionChanged += Cb_SelectionChanged;
                cb.ToolTip = App.Current.Resources["STR_CODE_Property_ValueList"] as String;
                return cb;
            }

            private void Cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                ComboBox cb = sender as ComboBox;
                PTypeDataTag tag = cb.Tag as PTypeDataTag;
                var data = SQF.ClassParser.File.ReceiveFieldFromHirarchy(tag.BaseData, tag.Path, true);
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
                    data.String = SqfProperty.SetSqfPropertySectionArg(tag.PropertyObject as SqfProperty, string.IsNullOrWhiteSpace(data.String) ? "" : data.String, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", value), (int)tag.Extra);
                }
                else
                {
                    data.Value = value;
                }
                TriggerValueChanged(data.Value);
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
