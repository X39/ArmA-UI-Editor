using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmA_UI_Editor.Code
{
    public class LimitedQueue<T> : IEnumerable<T>
    {
        private class Enumerator : IEnumerator<T>
        {
            private LimitedQueue<T> Parent;
            private int Index;
            public T Current { get { return Parent[this.Index]; } }
            object IEnumerator.Current { get { return Parent[this.Index]; } }

            public Enumerator(LimitedQueue<T> queue)
            {
                this.Parent = queue;
                this.Index = -1;
            }
            public void Dispose() { }
            public bool MoveNext()
            {
                return this.Parent.Count > ++this.Index;
            }

            public void Reset()
            {
                this.Index = -1;
            }
        }
        private T[] InnerArray;
        private int IndexOffsetStart;
        private int IndexOffsetEnd;
        public LimitedQueue(int size)
        {
            this.InnerArray = new T[size];
            this.IndexOffsetStart = 0;
            this.IndexOffsetEnd = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        public int Count { get { return this.IndexOffsetStart < this.IndexOffsetEnd ? this.MaxSize - this.IndexOffsetStart - this.IndexOffsetEnd : this.IndexOffsetStart - this.IndexOffsetEnd; } }
        public int MaxSize { get { return this.InnerArray.Length; } }
        public T this[int index]
        {
            get
            {
                if (index > this.Count)
                    throw new ArgumentOutOfRangeException();
                index += this.IndexOffsetEnd;
                if (index > this.MaxSize)
                    index -= this.MaxSize;
                return this.InnerArray[index];
            }
            set
            {
                if (index > this.Count)
                    throw new ArgumentOutOfRangeException();
                index += this.IndexOffsetEnd;
                if (index > this.MaxSize)
                    index -= this.MaxSize;
                this.InnerArray[index] = value;
            }
        }
        public void Push(T value)
        {
            this.InnerArray[this.IndexOffsetStart] = value;
            if (++this.IndexOffsetStart >= this.MaxSize)
            {
                this.IndexOffsetStart = 0;
            }
            if(this.IndexOffsetEnd == this.IndexOffsetStart)
            {
                this.IndexOffsetEnd--;
                if (++this.IndexOffsetEnd >= this.MaxSize)
                {
                    this.IndexOffsetEnd = 0;
                }
            }
        }
        public T Pop()
        {
            if (this.Count == 0)
                throw new ArgumentOutOfRangeException();
            var value = this.InnerArray[this.IndexOffsetEnd];
            this.InnerArray[this.IndexOffsetEnd] = default(T);
            if (++this.IndexOffsetEnd >= this.MaxSize)
            {
                this.IndexOffsetEnd = this.MaxSize - 1;
            }
            return value;
        }
    }
}
