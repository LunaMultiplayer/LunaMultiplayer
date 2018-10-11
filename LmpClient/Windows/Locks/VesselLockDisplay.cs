using LmpClient.Systems.SettingsSys;
using System;

namespace LmpClient.Windows.Locks
{
    internal class VesselLockDisplay
    {
        public Guid VesselId { get; set; }
        public string VesselName { get; set; }
        public bool Selected { get; set; }
        public bool Loaded { get; set; }
        public bool Packed { get; set; }
        public bool Immortal { get; set; }
        public string ControlLockOwner { get; set; }
        public string UpdateLockOwner { get; set; }
        public string UnloadedUpdateLockOwner { get; set; }

        public bool PlayerOwnsAnyLock()
        {
            return ControlLockOwner == SettingsSystem.CurrentSettings.PlayerName ||
                   UpdateLockOwner == SettingsSystem.CurrentSettings.PlayerName ||
                   UnloadedUpdateLockOwner == SettingsSystem.CurrentSettings.PlayerName;
        }
    }
}
