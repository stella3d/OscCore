using System.Runtime.InteropServices;

namespace OscCore
{
    static unsafe class PtrUtil
    {
        public static TPtr* Pin<TData, TPtr>(TData[] array, out GCHandle handle) 
            where TData: unmanaged
            where TPtr : unmanaged
        {
            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            return (TPtr*) handle.AddrOfPinnedObject();
        }
    }
}