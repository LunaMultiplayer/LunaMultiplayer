using System;
using System.Collections;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselWarpSys;
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

                    ClientSubspaceList[SettingsSystem.CurrentSettings.PlayerName] = value;
                    SendChangeSubspaceMsg(value);

                    if (value != -1 && !SkipSubspaceProcess)
                        ProcessNewSubspace();

                    VesselWarpSystem.Singleton.MovePlayerVesselsToNewSubspace(SettingsSystem.CurrentSettings.PlayerName, value);

                    Debug.Log($"[LMP]: Locked to subspace {value}, time: {GetCurrentSubspaceTime()}");
                }
            }
        }

        public bool NewSubspaceSent { get; set; }
        public Dictionary<string, int> ClientSubspaceList { get; } = new Dictionary<string, int>();
        public Dictionary<int, double> Subspaces { get; } = new Dictionary<int, double>();

        private ScreenMessage WarpMessage { get; set; }
        private WarpEvents WarpEvents { get; } = new WarpEvents();
        public bool SkipSubspaceProcess { get; set; }
        public bool WaitingSubspaceIdFromServer { get; set; }

        private const float UpdateScreenMessageSInterval = 0.2f;
        private const float CheckFollowMasterSInterval = 1f;

        #endregion

        #region Base overriden methods

        public override void OnDisabled()
        {
            GameEvents.onTimeWarpRateChanged.Remove(WarpEvents.OnTimeWarpChanged);
            ClientSubspaceList.Clear();
            Subspaces.Clear();
            _currentSubspace = int.MinValue;
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
            if (!NewSubspaceSent)
            {
                MessageSender.SendMessage(new WarpNewSubspaceMsgData
                {
                    SubspaceTimeDifference = Planetarium.GetUniversalTime() - TimeSyncerSystem.Singleton.GetServerClock(),
                    PlayerCreator = SettingsSystem.CurrentSettings.PlayerName,
                    //we don't send the subspaceKey as that one will be given by the server except when warping that we set it to -1
                });
            }
            NewSubspaceSent = true;
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

            VesselWarpSystem.Singleton.MovePlayerVesselsToNewSubspace(playerName, 0);
            //Move his vessels back to subspace 0
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
            Planetarium.SetUniversalTime(GetCurrentSubspaceTime());
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