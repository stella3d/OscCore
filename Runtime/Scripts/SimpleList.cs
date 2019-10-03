using System;
using System.Collections;
using System.Collections.Generic;

namespace OscCore
{
    public sealed class SimpleList<T> : IEnumerable<T>
    {
        public class Enumerator : IEnumerator<T>
        {
            SimpleList<T> m_Source;

            int m_Index;

            public Enumerator(SimpleList<T> list)
            {
                m_Source = list;
                m_Index = -1;
            }

            public bool MoveNext()
            {
                m_Index++;
                if (m_Index < m_Source.Count)
                {
                    Current = m_Source.Buffer[m_Index];
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                m_Index = -1;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                m_Source = null;
            }
        }
        
        readonly Enumerator m_Enumerator;
        
        public T[] Buffer;

        public int Count { get; private set; }
        public int Capacity => Buffer.Length;
                
        public SimpleList(int initialCapacity = 8)
        {
            m_Enumerator = new Enumerator(this);
            Buffer = new T[initialCapacity];
        }
        
        public void Add(T value)
        {
            if (Count >= Buffer.Length)
                Array.Resize(ref Buffer, Buffer.Length * 2);

            Buffer[Count] = value;
            Count++;
        }

        /// <summary>Reset count without wiping memory</summary>
        public void Reset()
        {
            Count = 0;
        }
        
        /// <summary>Reset count and wipe memory</summary>
        public void Clear()
        {
            Count = 0;
            for (var i = 0; i < Buffer.Length; i++)
                Buffer[i] = default;
        }

        public IEnumerator<T> GetEnumerator()
        {
            m_Enumerator.Reset();
            return m_Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}