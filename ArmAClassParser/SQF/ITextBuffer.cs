using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF
{
    public interface ITextBuffer
    {
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        int Length { get; }
        int Capacity { get; }
        bool PreventChanges { get; }

        char this[int i] { get; set; }
        void Append(System.IO.Stream v);
        void Append(object v);
        void Append(string v);
        void Append(char v);
        void Append(bool v);
        void Append(double v);
        void Append(long v);
        void Append(int v);
        void Append(short v);
        void Append(byte v);
        void AppendLine(string v);
        void Replace(string newContent, int startIndex, int oldLength);
        void Lock();
        void Unlock();
        string Substring(int thisOffset, int length);

    }
}
