using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UniLinq;

namespace LunaClient.Systems.Warp
{
    public class WarpSystem : MessageSystem<WarpSystem, WarpMessageSender, WarpMessageHandler>
    {
        #region Fields & properties

        public bool CurrentlyWarping => CurrentSubspace == -1;

        //public bool AloneInCurrentSubspace => !ClientSubspaceList.Any() || ClientSubspaceList.Count(p => p.Value == CurrentSubspace && p.Key != SettingsSystem.CurrentSettings.PlayerName) > 0;

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
                        ClientSubspaceList.TryAdd(SettingsSystem.CurrentSettings.PlayerName, _currentSubspace);
                    else
                        ClientSubspaceList[SettingsSystem.CurrentSettings.PlayerName] = _currentSubspace;

                    MessageSender.SendChangeSubspaceMsg(_currentSubspace);

                    if (_currentSubspace > 0 && !SkipSubspaceProcess)
                        ProcessNewSubspace();

                    SkipSubspaceProcess = false;

                    LunaLog.Log($"[LMP]: Locked to subspace {_currentSubspace}, time: {CurrentSubspaceTime}");
                }
            }
        }

        public ConcurrentDictionary<string, int> ClientSubspaceList { get; } = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<int, double> Subspaces { get; } = new ConcurrentDictionary<int, double>();
        public int LatestSubspace => Subspaces.Any() ? Subspaces.OrderByDescending(s => s.Value).First().Key : 0;
        private ScreenMessage WarpMessage { get; set; }
        private WarpEvents WarpEvents { get; } = new WarpEvents();
        public bool SkipSubspaceProcess { get; set; }
        public bool WaitingSubspaceIdFromServer { get; set; }
        public bool SyncedToLastSubspace { get; set; }
        private static DateTime StoppedWarpingTimeStamp { get; set; }

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(WarpSystem);

        protected override bool ProcessMessagesInUnityThread => false;

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
            if (SettingsSystem.ServerSettings.WarpMode != WarpMode.None)
            {
                SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, CheckWarpStopped));
                SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, CheckStuckAtWarp));
            }

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
        /// This routine checks if we are stuck at warping and if that's the case it request a new subspace again
        /// </summary>
        private void CheckStuckAtWarp()
        {
            if (CurrentSubspace == -1 && WaitingSubspaceIdFromServer && DateTime.Now - StoppedWarpingTimeStamp > TimeSpan.FromSeconds(15))
            {
                //We've waited for 15 seconds to get a subspace Id and the server didn't assigned one to us so send our subspace again...
                StoppedWarpingTimeStamp = DateTime.Now;
                RequestNewSubspace();
            }
        }

        /// <summary>
        /// This routine checks if we stopped warping.
        /// </summary>
        private void CheckWarpStopped()
        {
            if (TimeWarp.CurrentRateIndex == 0 && CurrentSubspace == -1 && !WaitingSubspaceIdFromServer)
            {
                RequestNewSubspace();
            }
        }

        /// <summary>
        /// Follows the warp master if the warp mode is set to MASTER and warp master is in another subspace
        /// </summary>
        private void FollowWarpMaster()
        {
            if (Enabled)
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
            if (Enabled)
            {
                DisplayMessage(SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName
                        ? $"{SettingsSystem.ServerSettings.WarpMaster} has warp control"
                        : "You have warp control", 1f);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Perform warp validations
        /// </summary>
        public bool WarpValidation()
        {
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.None || SettingsSystem.ServerSettings.WarpMode == WarpMode.Master &&
                SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName)
            {
                DisplayMessage(SettingsSystem.ServerSettings.WarpMode == WarpMode.None ?
                    LocalizationContainer.ScreenText.WarpDisabled :
                    LocalizationContainer.ScreenText.NotWarpMaster, 5f);

                return false;
            }


            if (WaitingSubspaceIdFromServer)
            {
                DisplayMessage(LocalizationContainer.ScreenText.WaitingSubspace, 5f);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Changes subspace if the given subspace is more advanced in time
        /// </summary>
        public void WarpIfSubspaceIsMoreAdvanced(int newSubspace)
        {
            if (Subspaces.TryGetValue(newSubspace, out var newSubspaceTime))
            {
                if (CurrentSubspaceTimeDifference < newSubspaceTime && CurrentSubspace != newSubspace)
                {
                    CoroutineUtil.StartDelayedRoutine(nameof(WarpIfSubspaceIsMoreAdvanced), () => CurrentSubspace = newSubspace, 0.5f);
                }
            }
        }

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

        /// <summary>
        /// Gets the current time on the subspace that we are located
        /// </summary>
        /// <returns></returns>
        public double CurrentSubspaceTime => GetSubspaceTime(CurrentSubspace);

        /// <summary>
        /// Gets the current time difference against the server time on the subspace that we are located
        /// </summary>
        /// <returns></returns>
        public double CurrentSubspaceTimeDifference => Subspaces.ContainsKey(CurrentSubspace) ? Subspaces[CurrentSubspace] : 0;

        /// <summary>
        /// Returns the subspace time sent as parameter.
        /// </summary>
        public double GetSubspaceTime(int subspace)
        {
            return Subspaces.ContainsKey(subspace) ? TimeSyncerSystem.ServerClockSec + Subspaces[subspace] : 0d;
        }


        public int GetPlayerSubspace(string playerName)
        {
            if (ClientSubspaceList.ContainsKey(playerName))
                return ClientSubspaceList[playerName];
            return 0;
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
                ClientSubspaceList.TryRemove(playerName, out _);
        }

        /// <summary>
        /// Here we warp and we set the time to the current subspace
        /// </summary>
        public void ProcessNewSubspace()
        {
            TimeWarp.fetch.WarpTo(CurrentSubspaceTime);
            ClockHandler.StepClock(CurrentSubspaceTime);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Task that requests a new subspace to the server.
        /// It has a sleep of 3 seconds to avoid errors like when you press "warp to next morning" or warp to a node
        /// </summary>
        private void RequestNewSubspace()
        {
            TaskFactory.StartNew(() =>
            {
                WaitingSubspaceIdFromServer = true;
                Thread.Sleep(3000);
                MessageSender.SendNewSubspace();
                StoppedWarpingTimeStamp = DateTime.Now;
            });
        }

        #endregion
    }
}
