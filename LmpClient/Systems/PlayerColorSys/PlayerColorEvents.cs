using System;
using LmpClient.Base;
using LmpClient.Extensions;
using LmpCommon.Locks;
using UniLinq;

namespace LmpClient.Systems.PlayerColorSys
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
        public void OnLockAcquire(LockDefinition lockDefinition)
        {
            if (lockDefinition.Type == LockType.Control)
                UpdateVesselColorsFromLockVesselId(lockDefinition.VesselId);
        }

        /// <summary>
        /// If we release the control of a ship et it's orbit color back to normal 
        /// </summary>
        public void OnLockRelease(LockDefinition lockDefinition)
        {
            if (lockDefinition.Type == LockType.Control)
                UpdateVesselColorsFromLockVesselId(lockDefinition.VesselId);
        }
        
        /// <summary>
        /// Called when you enter the map view.
        /// </summary>
        public void MapEntered()
        {
            foreach (var vessel in FlightGlobals.Vessels.Where(v=> v != null && v.orbitDriver?.Renderer != null))
            {
                System.SetVesselOrbitColor(vessel);
            }
        }

        /// <summary>
        /// Find the vessel using the lock name
        /// </summary>
        private static void UpdateVesselColorsFromLockVesselId(Guid vesselId)
        {
            var vessel = FlightGlobals.fetch.LmpFindVessel(vesselId);
            if (vessel != null)
            {
                System.SetVesselOrbitColor(vessel);
            }
        }
    }
}
