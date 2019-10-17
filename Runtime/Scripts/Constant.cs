using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;

namespace OscCore
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static unsafe class Constant
    {
        public const byte Comma = (byte) ',';
        public const byte ForwardSlash = (byte) '/';
        
        public static readonly byte[] BundlePrefixBytes;
        public static readonly long BundlePrefixLong;
        public static readonly byte* BundlePrefixPtr;

        static Constant()
        {
            var bundleBytes = Encoding.ASCII.GetBytes("#bundle ");
            bundleBytes[7] = 0;
            BundlePrefixBytes = bundleBytes;
            BundlePrefixLong = BitConverter.ToInt64(bundleBytes, 0);
            var handle = GCHandle.Alloc(BundlePrefixBytes, GCHandleType.Pinned);
            BundlePrefixPtr = (byte*) handle.AddrOfPinnedObject();
        }
    }
}