using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class VesselInitializeEvent : LmpBaseEvent
    {
        public static EventData<Vessel, bool> onVesselInitializing;
        public static EventData<Vessel, bool> onVesselInitialized;
    }
}
