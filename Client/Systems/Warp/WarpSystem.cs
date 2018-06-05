using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Localization;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UniLinq;

namespace LunaClient.Systems.Warp
{
    public class WarpSystem : MessageSystem<WarpSystem, WarpMessageSender, WarpMessageHandler>
    {
        #region Fields & properties

        private static DateTime _stoppedWarpingTimeStamp;

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

        public List<SubspaceDisplayEntry> SubspaceEntries { get; set; } = new List<SubspaceDisplayEntry>();

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
            SubspaceEntries.Clear();
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
            if (CurrentSubspace == -1 && WaitingSubspaceIdFromServer && TimeUtil.IsInInterval(ref _stoppedWarpingTimeStamp, 15000))
            {
                //We've waited for 15 seconds to get a subspace Id and the server didn't assigned one to us so send our subspace again...
                LunaLog.LogError("Detected stuck at warping! Requesting subspace ID again!");
                RequestNewSubspace();
            }
        }

        /// <summary>
        /// This routine checks if we stopped warping.
        /// </summary>
        private void CheckWarpStopped()
        {
            //Caution! When you use the "Warp to next morning" button and the warping is about to finish, 
            //the TimeWarp.CurrentRateIndex will be 0 but you will still be warping!! 
            //That's the reason why we check the TimeWarp.CurrentRate aswell!
            if (TimeWarp.CurrentRateIndex == 0 && Math.Abs(TimeWarp.CurrentRate - 1) < 0.1f && CurrentSubspace == -1 && !WaitingSubspaceIdFromServer)
            {
                WarpEvent.onTimeWarpStopped.Fire();
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
        public double CurrentSubspaceTimeDifference => Subspaces.TryGetValue(CurrentSubspace, out var time) ? time : 0;

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
            WarpMessage = LunaScreenMsg.PostScreenMessage(messageText, messageDuration, ScreenMessageStyle.UPPER_CENTER);
        }

        public void RemovePlayer(string playerName)
        {
            if (ClientSubspaceList.ContainsKey(playerName))
                ClientSubspaceList.TryRemove(playerName, out _);
        }

        /// <summary>
        /// Returns true if given subspace is equal or earlier in time than our subspace
        /// </summary>
        public bool SubspaceIsEqualOrInThePast(int subspaceId)
        {
            if (!CurrentlyWarping && CurrentSubspace == subspaceId)
                return true;

            if (subspaceId != -1 && Subspaces.TryGetValue(subspaceId, out var subspaceTime))
                return CurrentSubspaceTimeDifference > subspaceTime;

            return false;
        }

        public double GetTimeDifferenceWithGivenSubspace(int subspaceId)
        {
            if (subspaceId != -1 && Subspaces.TryGetValue(subspaceId, out var subspaceTime))
                return subspaceTime - CurrentSubspaceTimeDifference;

            return double.MaxValue;
        }

        /// <summary>
        /// Here we warp and we set the time to the current subspace
        /// </summary>
        public void ProcessNewSubspace()
        {
            TimeWarp.fetch.WarpTo(CurrentSubspaceTime);
            ClockHandler.StepClock(CurrentSubspaceTime);
            WarpEvent.onTimeWarpStopped.Fire();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Task that requests a new subspace to the server.
        /// </summary>
        private void RequestNewSubspace()
        {
            WaitingSubspaceIdFromServer = true;
            MessageSender.SendNewSubspace();
            _stoppedWarpingTimeStamp = LunaComputerTime.UtcNow;
        }

        #endregion
    }
}
