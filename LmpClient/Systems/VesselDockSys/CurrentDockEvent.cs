using System;

namespace LmpClient.Systems.VesselDockSys
{
    public static class CurrentDockEvent
    {
        public static DateTime DockingTime;
        public static Guid DominantVesselId;
        public static Guid WeakVesselId;

        public static void Reset()
        {
            DockingTime = DateTime.MinValue;
            DominantVesselId = Guid.Empty;
            WeakVesselId = Guid.Empty;
        }
    }
}
