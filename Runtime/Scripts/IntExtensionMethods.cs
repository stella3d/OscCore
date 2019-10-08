using System.Runtime.CompilerServices;

namespace OscCore
{
    public static class IntExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Align4(this int self)
        {
            return (self + 3) & ~3;
        }
    }
}