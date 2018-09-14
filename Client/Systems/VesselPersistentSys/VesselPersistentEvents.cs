using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.VesselPersistentSys
{
    public class VesselPersistentEvents : SubSystem<VesselPersistentSystem>
    {
        public void PartPersistentIdChanged(uint vesselPersistentId, uint from, uint to)
        {
            var vessel = GetChangingVesselLock(vesselPersistentId);
            if (vessel == null) return;

            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) || 
                FlightGlobals.ActiveVessel == vessel && !VesselCommon.IsSpectating)
                System.MessageSender.SendPartPersistantIdChanged(vesselPersistentId, from, to);
            else
            {
                VesselRemoveSystem.Singleton.KillVessel(vessel, "Changed persistent id on a not owned vessel");
            }
        }

        public void VesselPersistentIdChanged(uint from, uint to)
        {
            var vessel = GetChangingVesselLock(to);
            if (vessel == null) vessel = GetChangingVesselLock(from);
            if (vessel == null) return;

            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) ||
                FlightGlobals.ActiveVessel == vessel && !VesselCommon.IsSpectating)
                System.MessageSender.SendVesselPersistantIdChanged(from, to);
            else
            {
                VesselRemoveSystem.Singleton.KillVessel(vessel, "Changed persistent id on a not owned vessel");
            }
        }

        private Vessel GetChangingVesselLock(uint vesselPersistentId)
        {
            return FlightGlobals.FindVessel(vesselPersistentId, out var foundVessel) ? foundVessel : null;
        }

    }
}
