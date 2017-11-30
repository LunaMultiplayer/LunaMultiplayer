using System;
using System.Collections.Concurrent;

namespace LunaCommon.Time
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
            [TimeProvider.NtpOrg] = DateTime.MinValue,
        };

        /// <summary>
        /// Max requests are every 4 seconds, we use 5 for safety. Bear in mind that this check will only defend against several threads but on the SAME application
        /// </summary>
        internal static bool CanRequestTime(TimeProvider provider) => (DateTime.UtcNow - TimeProviderLastRequests[provider]).TotalSeconds > 5;

        /// <summary>
        /// Retrieves the date time from specified provider and defend against flooding
        /// </summary>
        internal static DateTime GetTime(TimeProvider provider, bool getAsLocalTime = false)
        {            
            //Max requests are every 4 seconds
            if (!CanRequestTime(provider))
                throw new Exception("Too many time requests!");

            DateTime dateTime;
            switch (provider)
            {
                case TimeProvider.Nist:
                    dateTime = TimeRetrieverNist.GetNistTime();
                    break;
                case TimeProvider.Microsoft:
                    dateTime = TimeRetrieverNtp.GetNtpTime("time.windows.com", getAsLocalTime);
                    break;
                case TimeProvider.NtpOrg:
                    dateTime = TimeRetrieverNtp.GetNtpTime("pool.ntp.org", getAsLocalTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }

            TimeProviderLastRequests[provider] = DateTime.UtcNow;

            return dateTime;
        }
    }
}