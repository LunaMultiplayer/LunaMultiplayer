using System;
using System.Collections;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.Warp
{
    public class WarpSystem : MessageSystem<WarpSystem, WarpMessageSender, WarpMessageHandler>
    {
        #region Fields

        public bool CurrentlyWarping => CurrentSubspace == -1;

        public bool AloneInCurrentSubspace => ClientSubspaceList.Count(p => p.Value == CurrentSubspace && p.Key != SettingsSystem.CurrentSettings.PlayerName) > 0;

        public WarpEntryDisplay WarpEntryDisplay { get; } = new WarpEntryDisplay();

        private int _currentSubspace = int.MinValue;
        public int CurrentSubspace
        {
            get { return _currentSubspace; }
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

        public int LatestSubspace => Subspaces.OrderByDescending(s => s.Value).Select(s => s.Key).First();

        private ScreenMessage WarpMessage { get; set; }
        private WarpEvents WarpEvents { get; } = new WarpEvents();
        public bool SkipSubspaceProcess { get; set; }
        public bool WaitingSubspaceIdFromServer { get; set; }

        private const float UpdateScreenMessageSInterval = 0.2f;
        private const float CheckFollowMasterSInterval = 1f;

        private bool SyncedToLastSubspace { get; set; } = false;

        #endregion

        #region Base overriden methods

        public override void Update()
        {
            base.Update();

            if (!SyncedToLastSubspace && MainSystem.Singleton.GameRunning && HighLogic.LoadedSceneIsGame && Time.timeSinceLevelLoad > 1f)
            {
                SyncToLatestSubspace();
                SyncedToLastSubspace = true;
            }
        }

        public override void OnDisabled()
        {
            GameEvents.onTimeWarpRateChanged.Remove(WarpEvents.OnTimeWarpChanged);
            ClientSubspaceList.Clear();
            Subspaces.Clear();
            _currentSubspace = int.MinValue;
            SkipSubspaceProcess = false;
            WaitingSubspaceIdFromServer = false;
            SyncedToLastSubspace = false;
        }

        public override void OnEnabled()
        {
            GameEvents.onTimeWarpRateChanged.Add(WarpEvents.OnTimeWarpChanged);

            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.MASTER &&
                !string.IsNullOrEmpty(SettingsSystem.ServerSettings.WarpMaster) &&
                SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName)
            {
                Client.Singleton.StartCoroutine(UpdateScreenMessage());
                Client.Singleton.StartCoroutine(FollowWarpMaster());
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

        public void SyncToLatestSubspace()
        {
            CurrentSubspace = LatestSubspace;
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
                PlayerCreator = SettingsSystem.CurrentSettings.PlayerName,
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
        /// Follows the warp master if the warp mode is set to MASTER and warp master is in another subspace
        /// </summary>
        private IEnumerator FollowWarpMaster()
        {
            var seconds = new WaitForSeconds(CheckFollowMasterSInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (MainSystem.Singleton.GameRunning)
                    {
                        if (ClientSubspaceList.ContainsKey(SettingsSystem.ServerSettings.WarpMaster) &&
                            ClientSubspaceList[SettingsSystem.ServerSettings.WarpMaster] != CurrentSubspace)
                        {
                            //Follow the warp master into warp if needed
                            CurrentSubspace = ClientSubspaceList[SettingsSystem.ServerSettings.WarpMaster];
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error in coroutine FollowWarpMaster {e}");
                    throw;
                }

                yield return seconds;
            }
        }

        /// <summary>
        /// Here we warp and we set the time to the current subspace
        /// </summary>
        private void ProcessNewSubspace()
        {
            TimeWarp.fetch.WarpTo(GetCurrentSubspaceTime());
            //TODO:Put StepClock in a more central place
            TimeSyncerSystem.StepClock(GetCurrentSubspaceTime());
        }

        /// <summary>
        /// Updates the screen message if warp mode is set to Master
        /// </summary>
        private IEnumerator UpdateScreenMessage()
        {
            var seconds = new WaitForSeconds(UpdateScreenMessageSInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (MainSystem.Singleton.GameRunning)
                    {
                        if (SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName)
                            DisplayMessage(SettingsSystem.ServerSettings.WarpMaster + " has warp control", 1f);
                        else
                            DisplayMessage("You have warp control", 1f);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error in coroutine CheckAbandonedVessels {e}");
                }

                yield return seconds;
            }
        }

        #endregion
    }
}