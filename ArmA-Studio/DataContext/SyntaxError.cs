using System;
using ICSharpCode.AvalonEdit.Document;

namespace ArmA.Studio.DataContext
{
    public class SyntaxError : ISegment
    {
        public int EndOffset { get { return this.StartOffset + Length; } }
        public int Offset { get { return this.StartOffset; } }

        public int Length { get; set; }
        public int StartOffset { get; set; }
        public string Message { get; set; }

        public static implicit operator TextSegment(SyntaxError sErr)
        {
            return new TextSegment() { StartOffset = sErr.StartOffset, Length = sErr.Length, EndOffset = sErr.EndOffset };
        }
    }
}