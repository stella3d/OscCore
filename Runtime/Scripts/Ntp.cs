using System;

namespace OscCore
{
    public class Ntp
    {
        public static System.DateTime NtpTimestampToDateTime(NtpTimestamp ts, DateTime? epoch = null)
        {
            long ticks = (ts.Seconds * TimeSpan.TicksPerSecond) + 
                                  (ts.Fractions * TimeSpan.TicksPerSecond) / 0x100000000L;

            return epoch != null ? epoch.Value + TimeSpan.FromTicks(ticks) : 
                    (ts.Seconds & 0x80000000L) == 0 ? 
                    Epoch2036 + TimeSpan.FromTicks(ticks) :
                    Epoch1900 + TimeSpan.FromTicks(ticks);
        }
        
        public static readonly DateTime Epoch2036 = new DateTime(2036, 2, 7, 6, 28, 16, DateTimeKind.Utc);

        public static DateTime Epoch1900 = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}