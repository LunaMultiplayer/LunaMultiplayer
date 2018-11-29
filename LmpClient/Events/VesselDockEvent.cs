using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class VesselDockEvent : LmpBaseEvent
    {
        public static EventData<Vessel, Vessel> onDocking;
        public static EventData<Vessel, Vessel> onDockingComplete;
    }
}
