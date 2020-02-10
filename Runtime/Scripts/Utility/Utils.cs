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
    }
}