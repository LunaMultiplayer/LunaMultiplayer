using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.VesselDecoupleSys
{
    public class VesselDecoupleEvents : SubSystem<VesselDecoupleSystem>
    {
        public void DecoupleStart(Part part, float breakForce)
        {
            if (VesselCommon.IsSpectating) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(part.vessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Detected decouple! Part: {part.partName} Vessel: {part.vessel.id}");
        }

        public void DecoupleComplete(Part part, float breakForce, Vessel originalVessel)
        {
            if (VesselCommon.IsSpectating) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Decouple complete! Part: {part.partName} Vessel: {part.vessel.id}");
            System.MessageSender.SendVesselDecouple(originalVessel, part.flightID, breakForce, part.vessel.id);

            LockSystem.Singleton.AcquireUnloadedUpdateLock(part.vessel.id, true, true);
            LockSystem.Singleton.AcquireUpdateLock(part.vessel.id, true, true);
        }
    }
}
