using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselImmortalSys
{
    public class VesselImmortalEvents : SubSystem<VesselImmortalSystem>
    {
        /// <summary>
        /// Set vessel immortal state just when the vessel loads
        /// </summary>
        public void VesselLoaded(Vessel vessel)
        {
            if(vessel == null) return;

            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == vessel.id)
                return;

            var isOurs = LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) ||
                LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName);

            System.SetVesselImmortalState(vessel, !isOurs);
        }

        /// <summary>
        /// Handles the vessel immortal state when someone gets a lock
        /// </summary>
        public void OnLockAcquire(LockDefinition lockDefinition)
        {
            if(lockDefinition.Type < LockType.Update) return;

            var vessel = FlightGlobals.FindVessel(lockDefinition.VesselId);
            if (!vessel.loaded) return;

            System.SetVesselImmortalState(vessel, lockDefinition.PlayerName != SettingsSystem.CurrentSettings.PlayerName);
        }

        /// <summary>
        /// Makes our active vessel mortal if we finished spectating
        /// </summary>
        public void FinishSpectating()
        {
            System.SetVesselImmortalState(FlightGlobals.ActiveVessel, false);
        }

        /// <summary>
        /// Makes our active vessel immortal if we are spectating
        /// </summary>
        public void StartSpectating()
        {
            System.SetVesselImmortalState(FlightGlobals.ActiveVessel, true);
        }
    }
}
