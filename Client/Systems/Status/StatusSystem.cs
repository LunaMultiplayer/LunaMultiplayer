using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.Status
{
    public class StatusSystem : MessageSystem<StatusSystem, StatusMessageSender, StatusMessageHandler>
    {
        #region Fields

        public PlayerStatus MyPlayerStatus { get; } = new PlayerStatus
        {
            PlayerName = SettingsSystem.CurrentSettings.PlayerName,
            StatusText = "Syncing"
        };

        public Dictionary<string, PlayerStatus> PlayerStatusList { get; } = new Dictionary<string, PlayerStatus>();

        private PlayerStatus LastPlayerStatus { get; } = new PlayerStatus();

        private bool StatusIsDifferent =>
            MyPlayerStatus.VesselText != LastPlayerStatus.VesselText ||
            MyPlayerStatus.StatusText != LastPlayerStatus.StatusText;

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(SettingsSystem.CurrentSettings.PlayerStatusCheckMsInterval,
                RoutineExecution.Update, CheckPlayerStatus));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PlayerStatusList.Clear();
            MyPlayerStatus.StatusText = "Syncing";
        }

        #endregion

        #region Public methods

        public int GetPlayerCount()
        {
            return PlayerStatusList.Count;
        }

        public PlayerStatus GetPlayerStatus(string playerName)
        {
            return PlayerStatusList.ContainsKey(playerName) ? PlayerStatusList[playerName] : null;
        }

        public void RemovePlayer(string playerToRemove)
        {
            if (PlayerStatusList.ContainsKey(playerToRemove))
            {
                PlayerStatusList.Remove(playerToRemove);
                Debug.Log($"[LMP]: Removed {playerToRemove} from Status list");
            }
            else
            {
                Debug.LogError($"[LMP]: Cannot remove non-existant player {playerToRemove}");
            }
        }

        #endregion

        #region Update methods

        private void CheckPlayerStatus()
        {
            if (Enabled && MainSystem.Singleton.GameRunning)
            {
                MyPlayerStatus.VesselText = GetVesselText();
                MyPlayerStatus.StatusText = GetStatusText();

                if (StatusIsDifferent)
                {
                    LastPlayerStatus.VesselText = MyPlayerStatus.VesselText;
                    LastPlayerStatus.StatusText = MyPlayerStatus.StatusText;

                    MessageSender.SendPlayerStatus(MyPlayerStatus);
                }
            }
        }

        #endregion

        #region Status getter

        private static string GetVesselText()
        {
            return !VesselCommon.IsSpectating && FlightGlobals.ActiveVessel != null
                ? FlightGlobals.ActiveVessel.vesselName
                : "";
        }

        private static string GetCurrentShipStatus()
        {
            var bodyName = VesselCommon.ActiveVesselIsInSafetyBubble() ? "safety bubble" : FlightGlobals.ActiveVessel.mainBody.bodyName;

            switch (FlightGlobals.ActiveVessel.situation)
            {
                case Vessel.Situations.DOCKED:
                    return $"Docked above {bodyName}";
                case Vessel.Situations.ESCAPING:
                    if (FlightGlobals.ActiveVessel.orbit.timeToPe < 0)
                        return $"Escaping {bodyName}";
                    return $"Encountering {bodyName}";
                case Vessel.Situations.FLYING:
                    return $"Flying above {bodyName}";
                case Vessel.Situations.LANDED:
                    return $"Landed on {bodyName}";
                case Vessel.Situations.ORBITING:
                    return $"Orbiting {bodyName}";
                case Vessel.Situations.PRELAUNCH:
                    return $"Launching from {bodyName}";
                case Vessel.Situations.SPLASHED:
                    return $"Splashed on {bodyName}";
                case Vessel.Situations.SUB_ORBITAL:
                    if (FlightGlobals.ActiveVessel.verticalSpeed > 0)
                        return $"Ascending from {bodyName}";
                    return $"Descending to {bodyName}";
                default:
                    return "Error";
            }
        }

        private static string GetExpectatingShipStatus()
        {
            if (LockSystem.Singleton.LockExists($"control-{FlightGlobals.ActiveVessel.id}"))
            {
                return LockSystem.Singleton.LockIsOurs($"control-{FlightGlobals.ActiveVessel.id}") ?
                    "Waiting for vessel control" : $"Spectating {LockSystem.Singleton.LockOwner($"control-{FlightGlobals.ActiveVessel.id}")}";
            }

            return "Spectating future updates";
        }

        private static string GetStatusText()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.FLIGHT:
                    if (FlightGlobals.ActiveVessel != null)
                        return !VesselCommon.IsSpectating ? GetCurrentShipStatus() : GetExpectatingShipStatus();
                    return "Loading";
                case GameScenes.EDITOR:
                    switch (EditorDriver.editorFacility)
                    {
                        case EditorFacility.VAB:
                            return "Building in VAB";
                        case EditorFacility.SPH:
                            return "Building in SPH";
                    }
                    return "Building";
                case GameScenes.SPACECENTER:
                    return "At Space Center";
                case GameScenes.TRACKSTATION:
                    return "At Tracking Station";
                case GameScenes.LOADING:
                    return "Loading";
                default:
                    return "Error";
            }
        }

        #endregion
    }
}