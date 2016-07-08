using System;

namespace SQF
{
    internal class TextBuffer : ITextBuffer, System.ComponentModel.INotifyPropertyChanged
    {
        public static implicit operator TextBuffer(string v)
        {
            var buffer = new TextBuffer();
            buffer.Append(v);
            return buffer;
        }
        private System.Text.StringBuilder Builder;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public int Length { get { return this.Builder.Length; } }
        public int Capacity { get { return this.Builder.Capacity; } set { this.Builder.Capacity = value; RaisePropertyChanged(); } }

        public bool PreventChanges { get; private set; }

        public char this[int i] { get { return this.Builder[i]; } set { if (PreventChanges) return; this.Builder[i] = value; RaisePropertyChanged(); } }

        public TextBuffer()
        {
            this.Builder = new System.Text.StringBuilder(1024 * 8);
        }

        public void Append(System.IO.Stream v) { if (PreventChanges) return; this.Builder.Append(new System.IO.StreamReader(v).ReadToEnd()); RaisePropertyChanged(); }
        public void Append(object v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(string v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(char v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(bool v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(double v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(long v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(int v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(short v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(byte v) { if (PreventChanges) return; this.Builder.Append(v); RaisePropertyChanged(); }

        public void AppendLine(string v) { if (PreventChanges) return; this.Builder.AppendLine(v); RaisePropertyChanged(); }

        public void Replace(string newContent, int startIndex, int oldLength)
        {
            if (PreventChanges)
                return;
            if (newContent.Length != oldLength)
            {
                if(newContent.Length > oldLength)
                {
                    for (int i = 0; i < oldLength; i++)
                    {
                        this.Builder[i + startIndex] = newContent[i];
                    }
                    this.Builder.Insert(startIndex + oldLength, newContent.Substring(oldLength));
                }
                else
                {
                    for (int i = 0; i < newContent.Length; i++)
                    {
                        this.Builder[i + startIndex] = newContent[i];
                    }
                    this.Builder.Remove(startIndex + newContent.Length, oldLength - newContent.Length);
                }
            }
            else
            {
                for(int i = 0; i < oldLength; i++)
                {
                    this.Builder[i + startIndex] = newContent[i];
                }
            }
        }

        public void Lock()
        {
            this.PreventChanges = true;
        }

        public void Unlock()
        {
            this.PreventChanges = false;
        }

        public override string ToString() { return this.Builder.ToString(); }
        public string Substring(int thisOffset, int length) { return this.Builder.ToString(thisOffset, length); }

    }
}