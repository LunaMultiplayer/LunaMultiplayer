using LunaClient.Base;
using LunaCommon.Locks;
using System;
using UniLinq;

namespace LunaClient.Systems.PlayerColorSys
{
    /// <summary>
    /// In this subsystem we handle the color events.
    /// When a player create a ship or acquire the lock of another ship the orbit color of it 
    /// will be changed to the player color. Also when releasing a lock the color must be swtiched to the default color
    /// </summary>
    public class PlayerColorEvents : SubSystem<PlayerColorSystem>
    {
        /// <summary>
        /// When we create a vessel set it's orbit color to the player color
        /// </summary>
        public void OnVesselCreated(Vessel colorVessel)
        {
            System.SetVesselOrbitColor(colorVessel);
        }

        /// <summary>
        /// If we acquire the control of a ship set it's orbit color
        /// </summary>
        public void OnLockAcquire(LockDefinition lockDefinition, bool result)
        {
            if (System.Enabled && lockDefinition.Type == LockType.Control && result)
                UpdateVesselColorsFromLockVesselId(lockDefinition.VesselId);
        }

        /// <summary>
        /// If we release the control of a ship et it's orbit color back to normal 
        /// </summary>
        public void OnLockRelease(LockDefinition lockDefinition)
        {
            if (System.Enabled && lockDefinition.Type == LockType.Control)
                UpdateVesselColorsFromLockVesselId(lockDefinition.VesselId);
        }

        /// <summary>
        /// Find the vessel using the lock name
        /// </summary>
        private void UpdateVesselColorsFromLockVesselId(Guid vesselId)
        {
            var vessel = FlightGlobals.FindVessel(vesselId);
            if (vessel != null)
            {
                System.SetVesselOrbitColor(vessel);
            }
        }
        
        /// <summary>
        /// Called when you enter the map view.
        /// </summary>
        public void MapEntered()
        {
            foreach (var vessel in FlightGlobals.Vessels.Where(v=> v.orbitDriver?.Renderer != null))
            {
                vessel.orbitDriver.Renderer.orbitColor = vessel.orbitDriver.orbitColor;
            }
        }
    }
}