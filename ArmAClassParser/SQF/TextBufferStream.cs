using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQF
{
    public class TextBufferStream : Stream
    {

        internal TextBuffer Buffer;
        private int Index = 0;

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { return this.Buffer.Length; } }
        public override long Position { get { return this.Index; } set { this.Index = (int)value; } }

        public TextBufferStream(TextBuffer buffer)
        {
            this.Buffer = buffer;
        }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.Buffer.Length == this.Index)
                return 0;
            for (int i = offset; i < offset + count; i++)
            {
                if (this.Buffer.Length == this.Index + 1)
                {
                    return i - offset;
                }
                buffer[i + offset] = (byte)this.Buffer[this.Index++];
            }
            return count;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Index = (int)offset;
                    break;
                case SeekOrigin.Current:
                    this.Index = (int)offset + this.Index;
                    break;
                case SeekOrigin.End:
                    this.Index = (int)offset + this.Buffer.Length;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return this.Index;
        }
        public override void SetLength(long value)
        {
            this.Buffer.Capacity = (int)value;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            for(int i = offset; i < offset + count; i++)
            {
                this.Buffer.Append((char)buffer[i]);
            }
        }

        public static implicit operator TextBufferStream(TextBuffer buffer)
        {
            return new TextBufferStream(buffer);
        }
    }
}
