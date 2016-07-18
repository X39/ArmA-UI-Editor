using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;

namespace ArmA_UI_Editor.Code
{
    public static class Utility
    {
        public static System.IO.MemoryStream AsMemoryStream(this string s)
        {
            var memStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(s));
            return memStream;
        }
        public static System.IO.Stream ToStream(this string str)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            return stream;
        }
        public static System.Windows.Rect GetCanvasMetrics(this System.Windows.UIElement el)
        {
            var rect = new System.Windows.Rect();
            rect.X = System.Windows.Controls.Canvas.GetLeft(el);
            rect.Y = System.Windows.Controls.Canvas.GetTop(el);
            rect.Width = System.Windows.Controls.Canvas.GetRight(el) - rect.X;
            rect.Height = System.Windows.Controls.Canvas.GetBottom(el) - rect.Y;
            return rect;
        }
        public static void SetCanvasMetrics(this System.Windows.UIElement el, System.Windows.Rect rect)
        {
            System.Windows.Controls.Canvas.SetLeft(el, rect.Left);
            System.Windows.Controls.Canvas.SetTop(el, rect.Top);
            System.Windows.Controls.Canvas.SetRight(el, rect.Right);
            System.Windows.Controls.Canvas.SetBottom(el, rect.Bottom);
        }
        public static bool IsNumeric(this string s)
        {
            bool dot = false;
            bool allowMinus = true;
            foreach (var c in s)
            {
                if (!char.IsDigit(c))
                {
                    if (c == '.' && !dot)
                    {
                        dot = true;
                    }
                    else if (c == '-' && allowMinus)
                    {
                        //empty
                    }
                    else
                    {
                        return false;
                    }
                }
                allowMinus = false;
            }
            return true;
        }
        public static bool IsValidIdentifier(this string s)
        {
            bool allowDigits = false;
            foreach (var c in s)
            {
                if (c == ' ')
                    return false;
                if(allowDigits)
                {
                    if (!char.IsLetterOrDigit(c) && c != '_')
                    {
                        return false;
                    }
                }
                else
                {
                    if (!char.IsLetter(c) && c != '_')
                    {
                        return false;
                    }
                    allowDigits = true;
                }
            }
            return true;
        }
        public static void ClearWhitespaces(this TextBox tb)
        {
            string text = tb.Text;
            if(text.Contains(' '))
            {
                int curStart = tb.SelectionStart;
                StringBuilder builder = new StringBuilder(text.Length);
                foreach(char c in text)
                {
                    if (c == ' ')
                    {
                        if (builder.Length < curStart)
                        {
                            curStart--;
                        }
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
                tb.Text = builder.ToString();
                tb.SelectionStart = curStart;
            }
        }


        public static void tb_PreviewTextInput_Numeric_DoHandle(object sender, TextCompositionEventArgs e, Action a)
        {
            TextBox tb = sender as TextBox;
            tb.ClearWhitespaces();
            if (e.Text.Contains('\r'))
            {
                if (tb.Text.Length == 0)
                    return;
                a.Invoke();
            }
            else
            {
                var currentSelection = tb.SelectionStart;
                string text = tb.Text;
                if (tb.SelectionLength > 0)
                {
                    text = text.Remove(tb.SelectionStart, tb.SelectionLength);
                }
                text = text.Insert(tb.SelectionStart, e.Text);
                currentSelection += e.Text.Length;
                if (text.IsNumeric())
                {
                    tb.Text = text;
                    tb.SelectionStart = currentSelection;
                }
            }
            e.Handled = true;
        }
        public static void tb_PreviewTextInput_Ident_DoHandle(object sender, TextCompositionEventArgs e, Action a)
        {
            TextBox tb = sender as TextBox;
            tb.ClearWhitespaces();
            if (e.Text.Contains('\r'))
            {
                if (tb.Text.Length == 0)
                    return;
                a.Invoke();
            }
            else
            {
                var currentSelection = tb.SelectionStart;
                string text = tb.Text;
                if (tb.SelectionLength > 0)
                {
                    text = text.Remove(tb.SelectionStart, tb.SelectionLength);
                }
                text = text.Insert(tb.SelectionStart, e.Text);
                currentSelection += e.Text.Length;
                if (text.IsValidIdentifier())
                {
                    tb.Text = text;
                    tb.SelectionStart = currentSelection;
                }
            }
            e.Handled = true;
        }
        public static T FindInVisualTreeUpward<T>(this DependencyObject el) where T : FrameworkElement
        {
            while (el != null && !(el is T))
                el = System.Windows.Media.VisualTreeHelper.GetParent(el);
            return el as T;
        }
        public static void ForceBindingSourceUpdate(this DependencyObject el, DependencyProperty prop)
        {
            var binding = BindingOperations.GetBindingExpression(el, prop);
            if(binding != null)
            {
                binding.UpdateSource();
            }
        }
    }
}
