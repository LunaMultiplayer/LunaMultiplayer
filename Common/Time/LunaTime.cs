using System;
using System.Threading;

namespace LunaCommon.Time
{
    /// <summary>
    /// Use this class to retrieve exact times
    /// </summary>
    public class LunaTime
    {
        /// <summary>
        /// Get correctly sync local time from internet
        /// </summary>
        public static DateTime Now => UtcNow.ToLocalTime();
        public static TimeSpan TimeDifference { get; private set; } = TimeSpan.Zero;

        private static readonly Mutex TimeMutex;
        private static readonly Timer Timer;

        /// <summary>
        /// We sync time with time provider every 30 seconds. This limits the number of clients + servers to 6
        /// </summary>
        private const int TimeSyncIntervalMs = 30 * 1000;

        /// <summary>
        /// Static constructor where we create the time that syncs time with the time providers every 10 seconds
        /// </summary>
        static LunaTime()
        {
            TimeMutex = new Mutex(false, "LunaTimeMutex");
            Timer = new Timer(_ => RefreshTimeDifference(), null, 0, TimeSyncIntervalMs);
        }

        /// <summary>
        /// Get correctly sync UTC time from internet
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow - TimeDifference;

        /// <summary>
        /// Here we refresh the time difference between our OS clock and the time providers clock.
        /// We use a mutex at OS level to prevent flooding the server and geting kicked
        /// </summary>
        private static void RefreshTimeDifference()
        {
            //In case we run several servers/clients we use a OS level mutex to avoid being kicked from the servers if we make too many requests
            if (TimeMutex.WaitOne(50))
            {
                try
                {
                    TimeDifference = DateTime.UtcNow - TimeRetriever.GetTime(TimeProvider.Nist);
                }
                catch (Exception)
                {
                    // ignored
                }

                //Make it sleep for 5 seconds to force other instances to advance the timer in case they try to flood the server
                Thread.Sleep(5000); 
                TimeMutex.ReleaseMutex();
            }
            else
            {
                //Advance the timer 5,5 seconds to avoid being kicked
                Timer.Change(5500, TimeSyncIntervalMs);
            }
        }
    }
}
