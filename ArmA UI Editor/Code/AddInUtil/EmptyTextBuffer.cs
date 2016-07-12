using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmA_UI_Editor.Code.AddInUtil
{
    public class EmptyTextBuffer : SQF.ITextBuffer
    {
        public char this[int i] { get { return '\0'; } set { } }
        public int Capacity { get { return int.MaxValue; } }
        public int Length { get { return 0; } }
        public bool PreventChanges { get { return true; } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void Append(int v) { }
        public void Append(byte v) { }
        public void Append(short v) { }
        public void Append(long v) { }
        public void Append(char v) { }
        public void Append(bool v) { }
        public void Append(double v) { }
        public void Append(string v) { }
        public void Append(object v) { }
        public void Append(Stream v) { }
        public void AppendLine(string v) { }
        public void Lock() { }
        public void Replace(string newContent, int startIndex, int oldLength) { }
        public string Substring(int thisOffset, int length)
        {
            return string.Empty;
        }
        public void Unlock() { }
        public void Insert(int index, string value) {}

        public int IndexOf(string v)
        {
            return -1;
        }
    }
}
