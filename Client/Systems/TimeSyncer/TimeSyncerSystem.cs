using System;
using System.Collections;
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

        #endregion

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            SyncSenderThread = new Thread(SyncTimeWithServer) {IsBackground = true};
            SyncSenderThread.Start();
            Client.Singleton.StartCoroutine(SyncTime());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            SyncSenderThread?.Abort();
            ServerStartTime = 0;
            ClockOffset.Clear();
            NetworkLatency.Clear();
            Synced = false;
            ClockOffsetAverage = 0;
            NetworkLatencyAverage = 0;
            ServerLag = 0;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Just before sending the msg to the server write the client send time
        /// </summary>
        public void RewriteMessage(IMessageData msg)
        {
            ((SyncTimeRequestMsgData) msg).ClientSendTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// This thread runs in the background and syncs the time with the server
        /// </summary>
        private void SyncTimeWithServer()
        {
            while (MainSystem.Singleton.NetworkState >= ClientState.CONNECTED)
            {
                MessageSender.SendTimeSyncRequest();
                Thread.Sleep(SettingsSystem.CurrentSettings.SyncTimeRequestMsInterval);
            }
        }

        /// <summary>
        /// Handles a sync time response and adjusts our clock to the server clock
        /// </summary>
        public void HandleSyncTime(long clientReceive, long clientSend, long serverReceive, long serverSend)
        {
            var clientLatency = clientReceive - clientSend - (serverSend - serverReceive);
            var clientOffset = (serverReceive - clientSend + (serverSend - clientReceive))/2;

            ClockOffset.Add(clientOffset);
            NetworkLatency.Add(clientLatency);
            ServerLag = serverSend - serverReceive;

            if (ClockOffset.Count > SyncTimeMax)
                ClockOffset.RemoveAt(0);
            if (NetworkLatency.Count > SyncTimeMax)
                NetworkLatency.RemoveAt(0);

            //Calculate the average for the offset and latency.
            var clockOffsetTotal = ClockOffset.Sum();
            ClockOffsetAverage = clockOffsetTotal/ClockOffset.Count;

            var networkLatencyTotal = NetworkLatency.Sum();
            NetworkLatencyAverage = networkLatencyTotal/NetworkLatency.Count;

            //Check if we are now synced
            if ((ClockOffset.Count > SettingsSystem.CurrentSettings.InitialConnectionSyncTimeRequests) && !Synced)
            {
                Synced = true;
                Debug.Log($"[LMP]: Initial clock syncronized, offset {ClockOffsetAverage/10000}ms, latency {NetworkLatencyAverage/10000}ms");
            }
        }

        public double GetServerClock()
            =>
            Synced ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ServerStartTime + ClockOffsetAverage).TotalSeconds : 0;

        public double GetCurrentError()
            => Synced ? Planetarium.GetUniversalTime() - WarpSystem.Singleton.GetCurrentSubspaceTime() : 0;

        #endregion

        #region Private methods

        /// <summary>
        /// Coroutine that checks our time against the server time and adjust it if needed.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SyncTime()
        {
            var seconds = new WaitForSeconds((float) TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.ClockSetMsInterval).TotalSeconds);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (Synced && !CurrentlyWarping && CanSyncTime() && !WarpSystem.Singleton.WaitingSubspaceIdFromServer)
                    {
                        var targetTime = WarpSystem.Singleton.GetCurrentSubspaceTime();
                        var currentError = TimeSpan.FromSeconds(GetCurrentError()).TotalMilliseconds;
                        if (Math.Abs(currentError) > MaxClockMsError)
                            Planetarium.SetUniversalTime(targetTime);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error in coroutine SyncTime {e}");
                }

                yield return seconds;
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