using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.VesselUndockSys
{
    public class VesselUndockEvents : SubSystem<VesselUndockSystem>
    {
        public void UndockStart(Part part, DockedVesselInfo dockedInfo)
        {
            if (VesselCommon.IsSpectating || System.IgnoreEvents) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(part.vessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Detected undock! Part: {part.partName} Vessel: {part.vessel.id}");
        }

        public void UndockComplete(Part part, DockedVesselInfo dockedInfo, Vessel originalVessel)
        {
            if (VesselCommon.IsSpectating || System.IgnoreEvents) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Undock complete! Part: {part} Vessel: {originalVessel.id}");
            System.MessageSender.SendVesselUndock(originalVessel, part.flightID, dockedInfo, part.vessel.id);

            LockSystem.Singleton.AcquireUnloadedUpdateLock(part.vessel.id, true, true);
            LockSystem.Singleton.AcquireUpdateLock(part.vessel.id, true, true);
        }
    }
}
