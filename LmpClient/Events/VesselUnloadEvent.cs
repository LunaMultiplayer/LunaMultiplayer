using LmpClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class VesselUnloadEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onVesselUnloading;
        public static EventData<Vessel> onVesselUnloaded;
    }
}
