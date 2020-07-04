using LmpClient.Events.Base;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class VesselAssemblyEvent : LmpBaseEvent
    {
        public static EventData<ShipConstruct> onAssemblingVessel;
        public static EventData<Vessel, ShipConstruct> onAssembledVessel;
        public static EventData<bool> onVesselValidationBeforAssembly;
    }
}
