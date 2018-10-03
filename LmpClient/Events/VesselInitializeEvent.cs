using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class VesselInitializeEvent : LmpBaseEvent
    {
        public static EventData<Vessel, bool> onVesselInitializing;
        public static EventData<Vessel, bool> onVesselInitialized;
    }
}
