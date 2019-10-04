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
    
    /// <summary>
    /// This class does no abstraction - It only keeps a counter associated with the array.
    /// All inserting should be done with the array reference and a for loop, then assign the count after the loop.
    /// </summary>
    /// <typeparam name="T1">The first type of data to store</typeparam>
    /// <typeparam name="T2">The second type of data to store</typeparam>
    /// <typeparam name="T3">The third type of data to store</typeparam>
    public sealed class Buffer<T1, T2, T3>
    {
        public int Count;
        public T1[] Values1;
        public T2[] Values2;
        public T3[] Values3;
        
        public Buffer(int initialCapacity = 8)
        {
            Count = 0;
            Values1 = new T1[initialCapacity];
            Values2 = new T2[initialCapacity];
            Values3 = new T3[initialCapacity];
        }
    }
}