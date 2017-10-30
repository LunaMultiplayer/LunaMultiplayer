using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Locks;
using System;
using UnityEngine;

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
        public void SetVesselOrbitColor(Vessel colorVessel)
        {
            if (System.Enabled)
            {
                if (LockSystem.LockQuery.ControlLockExists(colorVessel.id) &&
                    !LockSystem.LockQuery.ControlLockBelongsToPlayer(colorVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                {
                    var vesselOwner = LockSystem.LockQuery.GetControlLockOwner(colorVessel.id);
                    SetOrbitColor(colorVessel, System.GetPlayerColor(vesselOwner));
                }
                else
                {
                    SetOrbitColor(colorVessel, System.DefaultColor);
                }
            }
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
                SetVesselOrbitColor(vessel);
            }
        }
        
        /// <summary>
        /// Sets the orbit color in a vessel
        /// </summary>
        private static void SetOrbitColor(Vessel vessel, Color colour)
        {
            vessel.orbitDriver.orbitColor = colour;
            vessel.orbitDriver.Renderer.orbitColor = vessel.orbitDriver.orbitColor;
        }
    }
}