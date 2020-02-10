using System.Runtime.InteropServices;

namespace OscCore
{
    static class Utils
    {
        public static string ValidateAddress(string address)
        {
            if(string.IsNullOrEmpty(address)) 
                address = "/";
            if(address[0] != '/') address = 
                address.Insert(0, "/");
            if(address.EndsWith(" "))
                address = address.TrimEnd(' ');
            return address;
        }
        
        public static unsafe TPtr* PinPtr<TData, TPtr>(TData[] array, out GCHandle handle) 
            where TData: unmanaged
            where TPtr : unmanaged
        {
            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            return (TPtr*) handle.AddrOfPinnedObject();
        }
    }
}