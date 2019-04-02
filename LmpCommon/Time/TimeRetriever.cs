using System;
using System.Collections.Concurrent;

namespace LmpCommon.Time
{
    /// <summary>
    /// This class retrieves the exact time from internet
    /// </summary>
    internal class TimeRetriever
    {
        private static readonly ConcurrentDictionary<TimeProvider, DateTime> TimeProviderLastRequests = new ConcurrentDictionary<TimeProvider, DateTime>
        {
            [TimeProvider.Nist] = DateTime.MinValue,
            [TimeProvider.Microsoft] = DateTime.MinValue,
            [TimeProvider.Google] = DateTime.MinValue,
            [TimeProvider.NtpOrg] = DateTime.MinValue,
        };

        /// <summary>
        /// Max requests are every 4 seconds, we use 5 for safety. Bear in mind that this check will only defend against several threads but on the SAME application
        /// We don't use the LunaComputerTime as we need that this value is not affected by offsets
        /// </summary>
        internal static bool CanRequestTime(TimeProvider provider) => (DateTime.UtcNow - TimeProviderLastRequests[provider]).TotalSeconds > 5;

        /// <summary>
        /// Retrieves the date time from specified provider and defend against flooding
        /// </summary>
        internal static DateTime? GetTime(TimeProvider provider)
        {
            //Max requests are every 4 seconds
            if (!CanRequestTime(provider))
                throw new Exception("Too many time requests!");

            DateTime? dateTime;
            switch (provider)
            {
                case TimeProvider.Nist:
                    dateTime = TimeRetrieverNtp.GetNtpTime("time-a.nist.gov");
                    break;
                case TimeProvider.Microsoft:
                    dateTime = TimeRetrieverNtp.GetNtpTime("time.windows.com");
                    break;
                case TimeProvider.Google:
                    dateTime = TimeRetrieverNtp.GetNtpTime("time.google.com");
                    break;
                case TimeProvider.NtpOrg:
                    dateTime = TimeRetrieverNtp.GetNtpTime("pool.ntp.org");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }

            //We don't use the LunaComputerTime as we need that this value is not affected by offsets
            TimeProviderLastRequests[provider] = DateTime.UtcNow;

            return dateTime;
        }
    }
}
