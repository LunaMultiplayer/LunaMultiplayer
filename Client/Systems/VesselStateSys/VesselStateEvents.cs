using LunaClient.Base;

namespace LunaClient.Systems.VesselStateSys
{
    public class VesselStateEvents : SubSystem<VesselStateSystem>
    {
        public void OnVesselOnRails(Vessel data)
        {
            VesselStateSystem.VesselsOnRails.TryAdd(data.id, data);
            VesselStateSystem.VesselsOffRails.TryRemove(data.id, out _);
        }

        public void OnVesselOffRails(Vessel data)
        {
            VesselStateSystem.VesselsOnRails.TryRemove(data.id, out _);
            VesselStateSystem.VesselsOffRails.TryAdd(data.id, data);
        }
    }
}
