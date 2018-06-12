// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class VesselReloadEvent
    {
        public static EventData<Vessel> onVesselReloaded { get; } = new EventData<Vessel>("onVesselReloaded");
    }
}
