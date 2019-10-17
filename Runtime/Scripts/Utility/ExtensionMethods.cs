using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OscCore
{
    public static class ExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Align4(this int self)
        {
            return (self + 3) & ~3;
        }

        public static void SafeFree(this GCHandle handle)
        {
            if(handle.IsAllocated) handle.Free();
        }
    }
}