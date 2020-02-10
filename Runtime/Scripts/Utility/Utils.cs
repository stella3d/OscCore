using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OscCore
{
    static class Utils
    {
        // TODO - enforce ASCII / no special chars 
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
        
        public static bool ValidateAddress(ref string address)
        {
            if(string.IsNullOrEmpty(address)) 
                address = "/";
            if(address[0] != '/') address = 
                address.Insert(0, "/");
            if(address.EndsWith(" "))
                address = address.TrimEnd(' ');

            address = ReplaceInvalidAddressCharacters(address);
            return true;
        }

        static readonly List<char> k_TempChars = new List<char>();
        
        internal static string ReplaceInvalidAddressCharacters(string address)
        {
            k_TempChars.Clear();
            k_TempChars.AddRange(address.Where(OscParser.CharacterIsValidInAddress));
            return new string(k_TempChars.ToArray());
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