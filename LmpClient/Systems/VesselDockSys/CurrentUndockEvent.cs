using System;

namespace LmpClient.Systems.VesselDockSys
{
    public static class CurrentUndockEvent
    {
        public static Guid UndockingVesselId;

        public static void Reset()
        {
            UndockingVesselId = Guid.Empty;
        }
    }
}
