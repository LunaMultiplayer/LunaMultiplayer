using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselLockDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public string ControlLockOwner { get; set; }
        public string UpdateLockOwner { get; set; }
        public string UnloadedUpdateLockOwner { get; set; }

        public VesselLockDisplay(Guid vesselId) => VesselId = vesselId;

        public bool PlayerOwnsAnyLock()
        {
            return ControlLockOwner == SettingsSystem.CurrentSettings.PlayerName ||
                   UpdateLockOwner == SettingsSystem.CurrentSettings.PlayerName ||
                   UnloadedUpdateLockOwner == SettingsSystem.CurrentSettings.PlayerName;
        }

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            ControlLockOwner = LockSystem.LockQuery.GetControlLockOwner(VesselId);
            UpdateLockOwner = LockSystem.LockQuery.GetUpdateLockOwner(VesselId);
            UnloadedUpdateLockOwner = LockSystem.LockQuery.GetUnloadedUpdateLockOwner(VesselId);
        }

        protected override void PrintDisplay()
        {
            GUILayout.BeginHorizontal();
            StringBuilder.Length = 0;
            StringBuilder.Append("Control: ").AppendLine(ControlLockOwner)
                .Append("Update: ").AppendLine(UpdateLockOwner)
                .Append("UnlUpdate: ").Append(UnloadedUpdateLockOwner);

            GUILayout.Label(StringBuilder.ToString());
            GUILayout.FlexibleSpace();
            if (PlayerOwnsAnyLock())
            {
                if (GUILayout.Button("Release"))
                {
                    LockSystem.Singleton.ReleaseAllVesselLocks(null, VesselId);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
