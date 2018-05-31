using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
using LunaCommon;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Status
{
    public class StatusSystem : MessageSystem<StatusSystem, StatusMessageSender, StatusMessageHandler>
    {
        #region Fields

        public PlayerStatus MyPlayerStatus { get; } = new PlayerStatus();

        public ConcurrentDictionary<string, PlayerStatus> PlayerStatusList { get; } = new ConcurrentDictionary<string, PlayerStatus>();

        private PlayerStatus LastPlayerStatus { get; } = new PlayerStatus();

        private bool StatusIsDifferent =>
            MyPlayerStatus.VesselText != LastPlayerStatus.VesselText ||
            MyPlayerStatus.StatusText != LastPlayerStatus.StatusText;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(StatusSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            MyPlayerStatus.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            MyPlayerStatus.StatusText = "Syncing";

            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, CheckPlayerStatus));
            MessageSender.SendOwnStatus();
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
            if (playerName == SettingsSystem.CurrentSettings.PlayerName)
                return MyPlayerStatus;

            return PlayerStatusList.ContainsKey(playerName) ? PlayerStatusList[playerName] : null;
        }

        public void RemovePlayer(string playerToRemove)
        {
            if (PlayerStatusList.ContainsKey(playerToRemove))
            {
                PlayerStatusList.TryRemove(playerToRemove, out _);
            }
        }

        #endregion

        #region Update methods

        private void CheckPlayerStatus()
        {
            if (Enabled)
            {
                MyPlayerStatus.VesselText = GetVesselText();
                MyPlayerStatus.StatusText = GetStatusText();

                if (StatusIsDifferent)
                {
                    LastPlayerStatus.VesselText = MyPlayerStatus.VesselText;
                    LastPlayerStatus.StatusText = MyPlayerStatus.StatusText;

                    MessageSender.SendOwnStatus();
                }
            }
        }

        #endregion

        #region Status getter

        private static string GetVesselText()
        {
            return !VesselCommon.IsSpectating && FlightGlobals.ActiveVessel != null
                ? FlightGlobals.ActiveVessel.vesselName
                : string.Empty;
        }

        private static string GetCurrentShipStatus()
        {
            if (VesselCommon.IsInSafetyBubble(FlightGlobals.ActiveVessel))
                return "Inside safety bubble";

            switch (FlightGlobals.ActiveVessel.situation)
            {
                case Vessel.Situations.DOCKED:
                    return $"Docked above {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.ESCAPING:
                    if (FlightGlobals.ActiveVessel.orbit.timeToPe < 0)
                        return $"Escaping {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                    return $"Encountering {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.FLYING:
                    return $"Flying above {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.LANDED:
                    return $"Landed on {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.ORBITING:
                    return $"Orbiting {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.PRELAUNCH:
                    return $"Launching from {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.SPLASHED:
                    return $"Splashed on {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                case Vessel.Situations.SUB_ORBITAL:
                    if (FlightGlobals.ActiveVessel.verticalSpeed > 0)
                        return $"Ascending from {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                    return $"Descending to {FlightGlobals.ActiveVessel.mainBody.bodyName}";
                default:
                    return "Error";
            }
        }

        private static string GetExpectatingShipStatus()
        {
            if (LockSystem.LockQuery.ControlLockExists(FlightGlobals.ActiveVessel.id))
            {
                return LockSystem.LockQuery.ControlLockBelongsToPlayer(FlightGlobals.ActiveVessel.id, SettingsSystem.CurrentSettings.PlayerName) ?
                    "Waiting for vessel control" :
                    $"Spectating {LockSystem.LockQuery.GetControlLockOwner(FlightGlobals.ActiveVessel.id)}";
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
                    return "Other";
            }
        }

        #endregion
    }
}
