using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class RemoveEvent : LmpBaseEvent
    {
        public static EventData<Vessel> onLmpDestroyVessel;
        public static EventData<ProtoVessel> onLmpRecoveredVessel;
        public static EventData<ProtoVessel> onLmpTerminatedVessel;
    }
}
