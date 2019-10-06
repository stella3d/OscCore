using System.Collections;
using System.Collections.Generic;

namespace OscCore
{
    /// <summary>
    /// This class does no abstraction - It only keeps a counter associated with the array.
    /// All inserting should be done with the array reference and a for loop, then assign the count after the loop.
    /// </summary>
    /// <typeparam name="T">The type of data to store</typeparam>
    public sealed class Buffer<T>
    {
        public T[] Array;
        public int Count;
        
        public Buffer(int initialCapacity = 8)
        {
            Count = 0;
            Array = new T[initialCapacity];
        }
    }
    
    /// <summary>
    /// This class does no abstraction - It only keeps a counter associated with the array.
    /// All inserting should be done with the array reference and a for loop, then assign the count after the loop.
    /// </summary>
    /// <typeparam name="TKey">The first type of data to store</typeparam>
    /// <typeparam name="TValue">The second type of data to store</typeparam>
    public sealed class Buffer<TKey, TValue>
    {
        public int Count;
        public TKey[] Keys;
        public TValue[] Values;
        
        public Buffer(int initialCapacity = 8)
        {
            Count = 0;
            Keys = new TKey[initialCapacity];
            Values = new TValue[initialCapacity];
        }
    }
}