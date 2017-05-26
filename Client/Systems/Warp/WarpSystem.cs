using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.Warp
{
    public class WarpSystem : MessageSystem<WarpSystem, WarpMessageSender, WarpMessageHandler>
    {
        #region Fields & properties

        public bool CurrentlyWarping => CurrentSubspace == -1;

        public bool AloneInCurrentSubspace => ClientSubspaceList.Count(p => p.Value == CurrentSubspace && p.Key != SettingsSystem.CurrentSettings.PlayerName) > 0;

        public WarpEntryDisplay WarpEntryDisplay { get; } = new WarpEntryDisplay();

        private int _currentSubspace = int.MinValue;
        public int CurrentSubspace
        {
            get => _currentSubspace;
            set
            {
                if (_currentSubspace != value)
                {
                    _currentSubspace = value;

                    if (!ClientSubspaceList.ContainsKey(SettingsSystem.CurrentSettings.PlayerName))
                        ClientSubspaceList.Add(SettingsSystem.CurrentSettings.PlayerName, 0);

                    ClientSubspaceList[SettingsSystem.CurrentSettings.PlayerName] = value;
                    SendChangeSubspaceMsg(value);

                    if (value > 0 && !SkipSubspaceProcess)
                        ProcessNewSubspace();

                    SkipSubspaceProcess = false;

                    Debug.Log($"[LMP]: Locked to subspace {value}, time: {GetCurrentSubspaceTime()}");
                }
            }
        }

        public Dictionary<string, int> ClientSubspaceList { get; } = new Dictionary<string, int>();
        public Dictionary<int, double> Subspaces { get; } = new Dictionary<int, double>();

        public int LatestSubspace => Subspaces.Any() ?
            Subspaces.OrderByDescending(s => s.Value).Select(s => s.Key).First() : 0;

        private ScreenMessage WarpMessage { get; set; }
        private WarpEvents WarpEvents { get; } = new WarpEvents();
        public bool SkipSubspaceProcess { get; set; }
        public bool WaitingSubspaceIdFromServer { get; set; }
        public bool SyncedToLastSubspace { get; set; }

        #endregion

        #region Base overrides

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onTimeWarpRateChanged.Remove(WarpEvents.OnTimeWarpChanged);
            GameEvents.onLevelWasLoadedGUIReady.Remove(WarpEvents.OnSceneChanged);
            ClientSubspaceList.Clear();
            Subspaces.Clear();
            _currentSubspace = int.MinValue;
            SkipSubspaceProcess = false;
            WaitingSubspaceIdFromServer = false;
            SyncedToLastSubspace = false;
        }

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onTimeWarpRateChanged.Add(WarpEvents.OnTimeWarpChanged);
            GameEvents.onLevelWasLoadedGUIReady.Add(WarpEvents.OnSceneChanged);
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.Master &&
                !string.IsNullOrEmpty(SettingsSystem.ServerSettings.WarpMaster) &&
                SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName)
            {
                SetupRoutine(new RoutineDefinition(200, RoutineExecution.Update, UpdateScreenMessage));
                SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, FollowWarpMaster));
            }
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Follows the warp master if the warp mode is set to MASTER and warp master is in another subspace
        /// </summary>
        private void FollowWarpMaster()
        {
            if (Enabled && MainSystem.Singleton.GameRunning)
            {
                if (ClientSubspaceList.ContainsKey(SettingsSystem.ServerSettings.WarpMaster) &&
                    ClientSubspaceList[SettingsSystem.ServerSettings.WarpMaster] != CurrentSubspace)
                {
                    //Follow the warp master into warp if needed
                    CurrentSubspace = ClientSubspaceList[SettingsSystem.ServerSettings.WarpMaster];
                }
            }
        }

        /// <summary>
        /// Updates the screen message if warp mode is set to Master
        /// </summary>
        private void UpdateScreenMessage()
        {
            if (Enabled && MainSystem.Singleton.GameRunning)
            {
                if (SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName)
                    DisplayMessage(SettingsSystem.ServerSettings.WarpMaster + " has warp control", 1f);
                else
                    DisplayMessage("You have warp control", 1f);
            }
        }

        #endregion

        #region Public methods

        public bool PlayerIsInPastSubspace(string player)
        {
            if (ClientSubspaceList.ContainsKey(player) && CurrentSubspace >= 0)
            {
                var playerSubspace = ClientSubspaceList[player];
                if (playerSubspace == -1)
                    return false;

                return playerSubspace != CurrentSubspace && Subspaces[playerSubspace] < Subspaces[CurrentSubspace];
            }
            return false;
        }

        public double GetCurrentSubspaceTime() => GetSubspaceTime(CurrentSubspace);

        public double GetSubspaceTime(int subspace)
        {
            return TimeSyncerSystem.Singleton.Synced && Subspaces.ContainsKey(subspace)
                ? TimeSyncerSystem.Singleton.GetServerClock() + Subspaces[subspace]
                : 0;
        }

        public void SendChangeSubspaceMsg(int subspaceId)
        {
            MessageSender.SendMessage(new WarpChangeSubspaceMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                Subspace = subspaceId
            });
        }

        public int GetPlayerSubspace(string playerName)
        {
            if (ClientSubspaceList.ContainsKey(playerName))
                return ClientSubspaceList[playerName];
            return 0;
        }

        /// <summary>
        /// Sends the new subspace that we jumped into
        /// </summary>
        public void SendNewSubspace()
        {
            MessageSender.SendMessage(new WarpNewSubspaceMsgData
            {
                ServerTimeDifference = Planetarium.GetUniversalTime() - TimeSyncerSystem.Singleton.GetServerClock(),
                PlayerCreator = SettingsSystem.CurrentSettings.PlayerName
                //we don't send the SubspaceKey as that one will be given by the server except when warping that we set it to -1
            });
        }

        public void DisplayMessage(string messageText, float messageDuration)
        {
            if (WarpMessage != null)
                WarpMessage.duration = 0f;
            WarpMessage = ScreenMessages.PostScreenMessage(messageText, messageDuration, ScreenMessageStyle.UPPER_CENTER);
        }

        public void RemovePlayer(string playerName)
        {
            if (ClientSubspaceList.ContainsKey(playerName))
                ClientSubspaceList.Remove(playerName);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Here we warp and we set the time to the current subspace
        /// </summary>
        private void ProcessNewSubspace()
        {
            TimeWarp.fetch.WarpTo(GetCurrentSubspaceTime());
            ClockHandler.StepClock(GetCurrentSubspaceTime());
        }

        #endregion
    }
}