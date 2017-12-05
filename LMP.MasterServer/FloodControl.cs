using System;
using System.Collections.Concurrent;
using System.Net;

namespace LMP.MasterServer
{
    internal class FloodControl
    {
        internal static int MaxRequestsPerMs { get; set; } = 500;

        private static readonly ConcurrentDictionary<IPAddress, DateTime> FloodControlDictionary = new ConcurrentDictionary<IPAddress, DateTime>();

        public static bool AllowRequest(IPAddress address)
        {
            if (FloodControlDictionary.TryGetValue(address, out var lastRequest) && (DateTime.UtcNow - lastRequest).TotalMilliseconds < MaxRequestsPerMs)
            {
                return false;
            }

            FloodControlDictionary.AddOrUpdate(address, DateTime.UtcNow, (key, existingVal) => DateTime.UtcNow);
            return true;
        }
    }
}
