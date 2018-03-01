using LunaClient.Base;

namespace LunaClient.Systems.VesselStateSys
{
    public class VesselStateEvents : SubSystem<VesselStateSystem>
    {
        public void OnVesselOnRails(Vessel vessel)
        {
            LunaLog.Log($"Vessel {vessel.id} name {vessel.vesselName} gone ON rails");
            VesselStateSystem.VesselsOnRails.TryAdd(vessel.id, vessel);
            VesselStateSystem.VesselsOffRails.TryRemove(vessel.id, out _);
        }

        public void OnVesselOffRails(Vessel vessel)
        {
            LunaLog.Log($"Vessel {vessel.id} name {vessel.vesselName} gone OFF rails");
            VesselStateSystem.VesselsOnRails.TryRemove(vessel.id, out _);
            VesselStateSystem.VesselsOffRails.TryAdd(vessel.id, vessel);
        }
    }
}
