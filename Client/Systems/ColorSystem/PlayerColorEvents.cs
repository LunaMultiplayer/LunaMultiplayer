using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Utilities;

namespace LunaClient.Systems.ColorSystem
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
                if (LockSystem.Singleton.LockExists("control-" + colorVessel.id) && !LockSystem.Singleton.LockIsOurs("control-" + colorVessel.id))
                {
                    var vesselOwner = LockSystem.Singleton.LockOwner("control-" + colorVessel.id);
                    LunaLog.Debug($"Vessel {colorVessel.id} owner is {vesselOwner}");
                    colorVessel.orbitDriver.orbitColor = System.GetPlayerColor(vesselOwner);
                }
                else
                {
                    colorVessel.orbitDriver.orbitColor = System.DefaultColor;
                }
            }
        }

        /// <summary>
        /// If we acquire the control of a ship set it's orbit color
        /// </summary>
        public void OnLockAcquire(string playerName, string lockName, bool result)
        {
            if (System.Enabled && lockName.StartsWith("control-") && result)
                UpdateVesselColorsFromLockName(lockName);
        }

        /// <summary>
        /// If we release the control of a ship et it's orbit color back to normal 
        /// </summary>
        public void OnLockRelease(string playerName, string lockName)
        {
            if (System.Enabled && lockName.StartsWith("control-"))
                UpdateVesselColorsFromLockName(lockName);
        }

        /// <summary>
        /// Find the vessel using the lock name
        /// </summary>
        private void UpdateVesselColorsFromLockName(string lockName)
        {
            var vesselId = lockName.Substring(8);
            foreach (var findVessel in FlightGlobals.Vessels.Where(findVessel => findVessel.id.ToString() == vesselId))
            {
                SetVesselOrbitColor(findVessel);
            }
        }
    }
}