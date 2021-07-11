using System;
using System.Threading;

namespace LmpCommon.Time
{
    /// <summary>
    /// Use this class to retrieve exact times. All players and the server must have the same exact time so we adjust
    /// this class to get their internal clock errors against a NTP server
    /// </summary>
    public class LunaNetworkTime
    {
        /// <summary>
        /// Get correctly sync local time from internet
        /// </summary>
        public static DateTime Now => UtcNow.ToLocalTime();
        public static TimeSpan TimeDifference { get; private set; } = TimeSpan.Zero;
        public static float SimulatedMsTimeOffset { get; set; } = 0;

        private static readonly Timer Timer;

        /// <summary>
        /// We sync time with time provider every 30 seconds. This limits the number of clients/servers in the same machine to 6
        /// as the max queries a NTP server accept are 1 every 5 seconds
        /// </summary>
        private const int TimeSyncIntervalMs = 30 * 1000;

        /// <summary>
        /// Static constructor where we create the timer that syncs time with the time providers every 10 seconds
        /// </summary>
        static LunaNetworkTime() => Timer = new Timer(_ => RefreshTimeDifference(), null, 0, TimeSyncIntervalMs);

        /// <summary>
        /// Get correctly sync UTC time from internet
        /// </summary>
        public static DateTime UtcNow => LunaComputerTime.UtcNow + TimeDifference.Negate() + TimeSpan.FromMilliseconds(SimulatedMsTimeOffset);

        /// <summary>
        /// Here we refresh the time difference between our OS clock and the time providers clock.
        /// We use a mutex at OS level to prevent flooding the NTP server and getting kicked
        /// </summary>
        private static void RefreshTimeDifference()
        {
            //In case we run several servers/clients we use a OS level mutex to avoid being kicked from the servers if we make too many requests
            using (var timeMutex = new Mutex(true, "LunaTimeMutex", out var createdNew))
            {
                if (createdNew || timeMutex.WaitOne(10))
                {
                    //We OWN the mutex!
                    try
                    {
                        var ntpTime = TimeRetriever.GetTime(TimeProvider.Google);
                        if (ntpTime != null)
                            TimeDifference = LunaComputerTime.UtcNow - ntpTime.Value;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    //Make it sleep for 5 seconds to force other instances to advance the timer in case they try to flood the server
                    Thread.Sleep(5000);

                    //after all this, release the mutex so others can access it...
                    timeMutex.ReleaseMutex();
                }
                else
                {
                    //Advance the timer 5,5 seconds to avoid being kicked
                    Timer.Change(5500, TimeSyncIntervalMs);
                }
            }
        }
    }
}
