using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;

namespace LunaClient.Systems.VesselImmortalSys
{
    public class VesselImmortalEvents : SubSystem<VesselImmortalSystem>
    {        
        /// <summary>
        /// Set vessel immortal state just when the vessel goes on rails
        /// </summary>
        public void VesselGoOnRails(Vessel vessel)
        {
            System.SetImmortalStateBasedOnLock(vessel);
        }

        /// <summary>
        /// Set vessel immortal state just when the vessel goes off rails
        /// </summary>
        public void VesselGoOffRails(Vessel vessel)
        {
            System.SetImmortalStateBasedOnLock(vessel);
        }

        /// <summary>
        /// Set vessel immortal state just when the vessel loads
        /// </summary>
        public void VesselLoaded(Vessel vessel)
        {
            System.SetImmortalStateBasedOnLock(vessel);
        }

        /// <summary>
        /// Handles the vessel immortal state when someone gets a lock
        /// </summary>
        public void OnLockAcquire(LockDefinition lockDefinition)
        {
            if(lockDefinition.Type < LockType.Update) return;

            var vessel = FlightGlobals.FindVessel(lockDefinition.VesselId);

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
