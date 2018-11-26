using LmpClient.Base;
using LmpClient.Extensions;
using LmpCommon.Locks;

namespace LmpClient.Systems.VesselImmortalSys
{
    public class VesselImmortalEvents : SubSystem<VesselImmortalSystem>
    {        
        /// <summary>
        /// This event is called after a vessel has changed. Also called when starting a flight
        /// </summary>
        public void OnVesselChange(Vessel vessel)
        {
            System.SetImmortalStateBasedOnLock(vessel);
        }

        /// <summary>
        /// Set vessel immortal state just when the vessel has more/less parts (docking for example)
        /// </summary>
        public void PartCountChanged(Vessel vessel)
        {
            System.SetImmortalStateBasedOnLock(vessel);
        }

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
        /// Handles the vessel immortal state when someone gets an update or control lock
        /// </summary>
        public void OnLockAcquire(LockDefinition lockDefinition)
        {
            if(lockDefinition.Type != LockType.Control && lockDefinition.Type != LockType.Update && lockDefinition.Type != LockType.UnloadedUpdate) return;

            var vessel = FlightGlobals.FindVessel(lockDefinition.VesselId);
            System.SetImmortalStateBasedOnLock(vessel);
        }

        /// <summary>
        /// Handles the vessel immortal state when YOU release an update or control lock
        /// </summary>
        public void OnLockRelease(LockDefinition lockDefinition)
        {
            if (lockDefinition.Type != LockType.Control && lockDefinition.Type != LockType.Update && lockDefinition.Type != LockType.UnloadedUpdate) return;

            var vessel = FlightGlobals.FindVessel(lockDefinition.VesselId);
            System.SetImmortalStateBasedOnLock(vessel);
        }

        /// <summary>
        /// Makes our active vessel mortal if we finished spectating
        /// </summary>
        public void FinishSpectating()
        {
            System.SetImmortalStateBasedOnLock(FlightGlobals.ActiveVessel);
        }

        /// <summary>
        /// Makes our active vessel immortal if we are spectating
        /// </summary>
        public void StartSpectating()
        {
            System.SetImmortalStateBasedOnLock(FlightGlobals.ActiveVessel);
        }
    }
}
