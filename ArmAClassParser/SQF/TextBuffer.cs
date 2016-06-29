using System;

namespace ArmAClassParser.SQF
{
    public class TextBuffer : System.ComponentModel.INotifyPropertyChanged
    {
        private System.Text.StringBuilder Builder;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public int Length { get { return this.Builder.Length; } }
        public int Capacity { get { return this.Builder.Capacity; } set { this.Builder.Capacity = value; RaisePropertyChanged(); } }
        public char this[int i] { get { return this.Builder[i]; } set { this.Builder[i] = value; RaisePropertyChanged(); } }

        public TextBuffer()
        {
            this.Builder = new System.Text.StringBuilder(1024 * 8);
        }

        public void Append(object v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(string v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(char v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(bool v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(double v) { this.Builder.Append(v); RaisePropertyChanged(); }

        internal string Substring(int thisOffset, int length)
        {
            return this.Builder.ToString(thisOffset, length);
        }

        public void Append(long v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(int v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(short v) { this.Builder.Append(v); RaisePropertyChanged(); }
        public void Append(byte v) { this.Builder.Append(v); RaisePropertyChanged(); }

        public override string ToString()
        {
            return this.Builder.ToString();
        }

        public void Replace(string newContent, int startIndex, int oldLength)
        {
            if(newContent.Length != oldLength)
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
    }
}