using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.TimeSyncer
{
    /// <summary>
    /// This class syncs the server time with your time.
    /// Bear in mind that it's a bit complex without using NTP protocol (https://en.wikipedia.org/wiki/Network_Time_Protocol)
    /// Therefore we use the trip time to do some aproximations. It's not perfect but it's enough.
    /// More info: http://www.mine-control.com/zack/timesync/timesync.html
    /// </summary>
    public class TimeSyncerSystem : MessageSystem<TimeSyncerSystem, TimeSyncerMessageSender, TimeSyncerMessageHandler>
    {
        #region Fields

        #region Public

        public long ServerStartTime { get; set; }
        public bool Synced { get; private set; }
        public long ClockOffsetAverage { get; private set; }
        public long NetworkLatencyAverage { get; private set; }
        public long ServerLag { get; private set; }

        private long LastTimeSync { get; set; }

        #endregion

        #region Constants

        private const int MaxClockMsError = 100;
        private const int SyncTimeMax = 100;
        
        #endregion

        #region Private

        private List<long> ClockOffset { get; } = new List<long>();
        private List<long> NetworkLatency { get; } = new List<long>();
        public Thread SyncSenderThread { get; set; }

        private static bool CurrentlyWarping => WarpSystem.Singleton.CurrentSubspace == -1;

        private bool _enabled;
        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                {
                    SyncSenderThread = new Thread(SyncTimeWithServer) {IsBackground = true};
                    SyncSenderThread.Start();
                }
                else if (_enabled && !value)
                    SyncSenderThread?.Abort();

                _enabled = value;
            }
        }

        #endregion

        #endregion

        #region Base overrides

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Enabled && Synced && !CurrentlyWarping && CanSyncTime())
            {
                SyncTime();
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Just before sending the msg to the server write the client send time
        /// </summary>
        public void RewriteMessage(IMessageData msg)
        {
            ((SyncTimeRequestMsgData)msg).ClientSendTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// This thread runs in the background and syncs the time with the server
        /// </summary>
        private void SyncTimeWithServer()
        {
            while (MainSystem.Singleton.NetworkState >= ClientState.CONNECTED)
            {
                MessageSender.SendTimeSyncRequest();
                MainSystem.Delay(SettingsSystem.CurrentSettings.SyncTimeRequestMsInterval);
            }
        }

        /// <summary>
        /// Handles a sync time response and adjusts our clock to the server clock
        /// </summary>
        public void HandleSyncTime(long clientReceive, long clientSend, long serverReceive, long serverSend)
        {
            var clientLatency = clientReceive - clientSend - (serverSend - serverReceive);
            var clientOffset = (serverReceive - clientSend + (serverSend - clientReceive)) / 2;

            ClockOffset.Add(clientOffset);
            NetworkLatency.Add(clientLatency);
            ServerLag = serverSend - serverReceive;

            if (ClockOffset.Count > SyncTimeMax)
                ClockOffset.RemoveAt(0);
            if (NetworkLatency.Count > SyncTimeMax)
                NetworkLatency.RemoveAt(0);

            //Calculate the average for the offset and latency.
            var clockOffsetTotal = ClockOffset.Sum();
            ClockOffsetAverage = clockOffsetTotal / ClockOffset.Count;

            var networkLatencyTotal = NetworkLatency.Sum();
            NetworkLatencyAverage = networkLatencyTotal / NetworkLatency.Count;

            //Check if we are now synced
            if ((ClockOffset.Count > SettingsSystem.CurrentSettings.InitialConnectionSyncTimeRequests) && !Synced)
            {
                Synced = true;
                LunaLog.Debug($"Initial clock syncronized, offset {ClockOffsetAverage / 10000}ms, latency {NetworkLatencyAverage / 10000}ms");
            }
        }
        
        public double GetServerClock() => Synced ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ServerStartTime + ClockOffsetAverage).TotalSeconds : 0;

        public double GetCurrentError() => Synced ? Planetarium.GetUniversalTime() - WarpSystem.Singleton.GetCurrentSubspaceTime() : 0;
        
        #endregion

        #region Private methods

        private void SyncTime()
        {
            if (DateTime.UtcNow.Ticks - LastTimeSync > TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.ClockSetMsInterval).Ticks)
            {
                LastTimeSync = DateTime.UtcNow.Ticks;
                var targetTime = WarpSystem.Singleton.GetCurrentSubspaceTime();
                var currentError = GetCurrentError();
                if (Math.Abs(currentError) > MaxClockMsError)
                    Planetarium.SetUniversalTime(targetTime);
            }
        }
        
        private static bool CanSyncTime()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.TRACKSTATION:
                case GameScenes.FLIGHT:
                case GameScenes.SPACECENTER:
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}