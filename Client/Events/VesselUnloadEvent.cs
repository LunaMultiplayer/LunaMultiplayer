// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class VesselUnloadEvent
    {
        public static EventData<Vessel> onVesselUnloading { get; } = new EventData<Vessel>("onVesselUnloading");
        public static EventData<Vessel> onVesselUnloaded { get; } = new EventData<Vessel>("onVesselUnloaded");
    }
}
