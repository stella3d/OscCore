using System;
using JetBrains.Annotations;

namespace OscCore
{
    public class OscActionPair
    {
        /// <summary>
        /// This callback runs immediately on the data receiving thread
        /// </summary>
        public Action<OscMessageValues> ValueRead;
        
        /// <summary>
        /// An event queued on the main thread for the next frame
        /// </summary>
        public Action MainThreadQueued;

        public OscActionPair(Action<OscMessageValues> valueRead, Action mainThreadQueued)
        {
            const string nullWarning = "Value read callbacks required!";
            ValueRead = valueRead ?? throw new ArgumentNullException(nameof(valueRead), nullWarning);
            // main thread callback is optional
            MainThreadQueued = mainThreadQueued;
        }
        
        public static OscActionPair operator + (OscActionPair l, OscActionPair r)
        {
            var mainThread = l.MainThreadQueued == null ? r.MainThreadQueued : l.MainThreadQueued + r.MainThreadQueued;
            var valueRead = l.ValueRead + r.ValueRead;
            return new OscActionPair(valueRead, mainThread);
        }
        
        public static OscActionPair operator - (OscActionPair l, OscActionPair r)
        {
            var mainThread = l.MainThreadQueued == null ? r.MainThreadQueued : l.MainThreadQueued - r.MainThreadQueued;
            var valueRead = l.ValueRead - r.ValueRead;
            return new OscActionPair(valueRead, mainThread);
        }
    }
}