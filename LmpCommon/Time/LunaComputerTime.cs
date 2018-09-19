using System;

namespace LmpCommon.Time
{
    /// <summary>
    /// Use this class to retrieve times in the local computer and add a simulated offset
    /// </summary>
    public class LunaComputerTime
    {
        public static DateTime Now => UtcNow.ToLocalTime();
        public static float SimulatedMinutesTimeOffset { get; set; } = 0;
        public static DateTime UtcNow => DateTime.UtcNow + TimeSpan.FromMinutes(SimulatedMinutesTimeOffset);
    }
}
