using System;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.Warp
{
    public class WarpSystem : MessageSystem<WarpSystem, WarpMessageSender, WarpMessageHandler>
    {
        #region Fields

        private bool _enabled;
        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                {
                    RegisterGameHooks();
                }
                else if (_enabled && !value)
                {
                    UnregisterGameHooks();
                    ClientSubspaceList.Clear();
                    Subspaces.Clear();
                }

                _enabled = value;
            }
        }

        public bool CurrentlyWarping => CurrentSubspace == -1;
        public bool AloneInCurrentSubspace => ClientSubspaceList
            .Count(p => p.Value == CurrentSubspace && p.Key != SettingsSystem.CurrentSettings.PlayerName) > 0;

        public WarpEntryDisplay WarpEntryDisplay { get; } = new WarpEntryDisplay();

        private int _currentSubspace;
        public int CurrentSubspace
        {
            get { return _currentSubspace; }
            set
            {
                ClientSubspaceList[SettingsSystem.CurrentSettings.PlayerName] = value;
                SendChangeSubspaceMsg(value);

                if (value != -1 && value != _currentSubspace && !SkipSubspaceProcess)
                    ProcessNewSubspace();

                VesselWarpSystem.Singleton.MovePlayerVesselsToNewSubspace(SettingsSystem.CurrentSettings.PlayerName, value);
                _currentSubspace = value;
                LunaLog.Debug("Locked to subspace " + value + ", time: " + GetCurrentSubspaceTime());
            }
        }

        public bool NewSubspaceSent { get; set; }
        public Dictionary<string, int> ClientSubspaceList { get; } = new Dictionary<string, int>();
        public Dictionary<int, double> Subspaces { get; } = new Dictionary<int, double>();

        private double LastScreenMessageCheck { get; set; }
        private ScreenMessage WarpMessage { get; set; }
        private WarpEvents WarpEvents { get; } = new WarpEvents();
        public bool SkipSubspaceProcess { get; set; }

        private const float UpdateScreenMessageInterval = 0.2f;

        #endregion

        #region Base overriden methods
        
        public override void Update()
        {
            base.Update();
            if (!Enabled)
                return;
            
            UpdateScreenMessage();
            FollowWarpMaster();
        }
        
        #endregion

        #region Public methods
        
        public double GetCurrentSubspaceTime() => GetSubspaceTime(CurrentSubspace);

        public double GetSubspaceTime(int subspace)
        {
            return TimeSyncerSystem.Singleton.Synced && Subspaces.ContainsKey(subspace) ? 
                TimeSyncerSystem.Singleton.GetServerClock() + Subspaces[subspace] : 0;
        }

        public void SendChangeSubspaceMsg(int subspaceId)
        {
            MessageSender.SendMessage(new WarpChangeSubspaceMsgData { PlayerName = SettingsSystem.CurrentSettings.PlayerName, Subspace = subspaceId });
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
                    SubspaceTime = Planetarium.GetUniversalTime(),
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

            VesselWarpSystem.Singleton.MovePlayerVesselsToNewSubspace(playerName, 0); //Move his vessels back to subspace 0
        }

        #endregion

        #region Private methods
        
        private void UnregisterGameHooks()
        {
            GameEvents.onTimeWarpRateChanged.Remove(WarpEvents.OnTimeWarpChanged);
        }

        private void RegisterGameHooks()
        {
            GameEvents.onTimeWarpRateChanged.Add(WarpEvents.OnTimeWarpChanged);
        }

        /// <summary>
        /// Follows the warp master if the warp mode is set to MASTER and warp master is in another subspace
        /// </summary>
        private void FollowWarpMaster()
        {
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.MASTER &&
                SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName &&
                ClientSubspaceList.ContainsKey(SettingsSystem.ServerSettings.WarpMaster) &&
                ClientSubspaceList[SettingsSystem.ServerSettings.WarpMaster] != CurrentSubspace)
            {
                //Follow the warp master into warp if needed
                CurrentSubspace = ClientSubspaceList[SettingsSystem.ServerSettings.WarpMaster];
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
        private void UpdateScreenMessage()
        {
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.MASTER && !string.IsNullOrEmpty(SettingsSystem.ServerSettings.WarpMaster) && 
                Time.realtimeSinceStartup - LastScreenMessageCheck > UpdateScreenMessageInterval)
            {
                LastScreenMessageCheck = Time.realtimeSinceStartup;

                if (SettingsSystem.ServerSettings.WarpMaster != SettingsSystem.CurrentSettings.PlayerName)
                    DisplayMessage(SettingsSystem.ServerSettings.WarpMaster + " has warp control", 1f);
                else
                    DisplayMessage("You have warp control", 1f);
            }
        }
        
        #endregion
    }
}