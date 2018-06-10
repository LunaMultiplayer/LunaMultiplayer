// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class VesselLoadEvent
    {
        public static EventData<Vessel> onVesselLoaded { get; } = new EventData<Vessel>("onVesselLoaded");
    }
}
