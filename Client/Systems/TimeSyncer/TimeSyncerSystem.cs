using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LunaClient.Systems.TimeSyncer
{
    /// <summary>
    /// This system syncs the server time with your time.
    /// </summary>
    public class TimeSyncerSystem : MessageSystem<TimeSyncerSystem, TimeSyncerMessageSender, TimeSyncerMessageHandler>
    {
        #region Fields & properties

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
        private const float MinClockRate = 0.3f;
        private const float MaxClockRate = 1.5f;
        private const int MaxClockSkew = 5000;

        #endregion

        #region Private

        private static readonly object ListLock = new object();

        private List<long> ClockOffset { get; } = new List<long>();
        private List<long> NetworkLatency { get; } = new List<long>();
        public Task SyncSenderThread { get; private set; }

        private static bool CurrentlyWarping => SystemsContainer.Get<WarpSystem>().CurrentSubspace == -1;

        #endregion

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SyncSenderThread = LongRunTaskFactory.StartNew(SyncTimeWithServer);
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.ClockSetMsInterval, RoutineExecution.Update, SyncTime));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            if (!SyncSenderThread.IsCompleted)
                SyncSenderThread.Wait(500);
            ServerStartTime = 0;
            ClockOffset.Clear();
            NetworkLatency.Clear();
            Synced = false;
            ClockOffsetAverage = 0;
            NetworkLatencyAverage = 0;
            ServerLag = 0;
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Routine that checks our time against the server time and adjust it if needed.
        /// </summary>
        /// <returns></returns>
        private void SyncTime()
        {
            //TODO: Improve performance 5ms max
            if (Enabled && Synced && !CurrentlyWarping && CanSyncTime() && !SystemsContainer.Get<WarpSystem>().WaitingSubspaceIdFromServer)
            {
                var targetTime = (int)SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime();
                var currentError = TimeSpan.FromSeconds(GetCurrentError()).TotalMilliseconds;
                if (targetTime != 0 && Math.Abs(currentError) > MaxClockMsError)
                {
                    if (Math.Abs(currentError) > MaxClockSkew)
                    {
                        LunaLog.LogWarning($"[LMP] Adjusted time from: {Planetarium.GetUniversalTime()} to: {targetTime} due to error:{currentError}");
                        //TODO: This causes the throttle to reset when called.  This happens due to vessel unpacking resetting the throttle controls.
                        //TODO: Try to get Squad to change their code.
                        ClockHandler.StepClock(targetTime);
                    }
                    else
                    {
                        SkewClock(currentError);
                    }
                }
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
            while (MainSystem.NetworkState >= ClientState.Authenticated)
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
            lock (ListLock)
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
                if (ClockOffset.Count > SettingsSystem.CurrentSettings.InitialConnectionSyncTimeRequests && !Synced)
                {
                    Synced = true;
                    LunaLog.Log(
                        $"[LMP]: Initial clock syncronized, offset {ClockOffsetAverage / 10000}ms, latency {NetworkLatencyAverage / 10000}ms");
                }
            }
        }

        public double GetServerClock() => Synced ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ServerStartTime + ClockOffsetAverage).TotalSeconds : 0;

        public double GetCurrentError()
            => Synced ? Planetarium.GetUniversalTime() - SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime() : 0;

        #endregion

        #region Private methods

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



        private static void SkewClock(double currentError)
        {
            var timeWarpRate = (float)Math.Pow(2, -(currentError / 1000f));

            if (timeWarpRate > MaxClockRate)
            {
                timeWarpRate = MaxClockRate;
            }
            else if (timeWarpRate < MinClockRate)
            {
                timeWarpRate = MinClockRate;
            }

            //Set the physwarp rate
            Time.timeScale = timeWarpRate;
        }

        #endregion
    }
}