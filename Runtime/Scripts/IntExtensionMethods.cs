namespace OscCore
{
    public static class IntExtensionMethods
    {
        public static int Align4(this int self)
        {
            var quotient = self / 4;
            var multipliedQuotient = quotient * 4;
            if (quotient == multipliedQuotient)
                return self;
            
            return self + 4 -(self - multipliedQuotient);
        }
    }
}