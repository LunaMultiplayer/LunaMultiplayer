using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private const float MIN_CLOCK_RATE = 0.3f;
        private const float MAX_CLOCK_RATE = 1.5f;
        private const int MAX_CLOCK_SKEW = 5000;

        #endregion

        #region Private

        private List<long> ClockOffset { get; } = new List<long>();
        private List<long> NetworkLatency { get; } = new List<long>();
        public Task SyncSenderThread { get; private set; }

        private static bool CurrentlyWarping => WarpSystem.Singleton.CurrentSubspace == -1;

        #endregion

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            SyncSenderThread = new Task(SyncTimeWithServer);
            SyncSenderThread.Start(TaskScheduler.Default);
            Client.Singleton.StartCoroutine(SyncTime());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            SyncSenderThread?.Wait(500);
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
            ((SyncTimeRequestMsgData)msg).ClientSendTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// This thread runs in the background and syncs the time with the server
        /// </summary>
        private void SyncTimeWithServer()
        {
            while (!MainSystem.Singleton.Quit && MainSystem.Singleton.NetworkState >= ClientState.AUTHENTICATED)
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
                Debug.Log($"[LMP]: Initial clock syncronized, offset {ClockOffsetAverage / 10000}ms, latency {NetworkLatencyAverage / 10000}ms");
            }
        }

        public double GetServerClock() => Synced ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ServerStartTime + ClockOffsetAverage).TotalSeconds : 0;

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
            var seconds = new WaitForSeconds((float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.ClockSetMsInterval).TotalSeconds);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (Synced && !CurrentlyWarping && CanSyncTime() && !WarpSystem.Singleton.WaitingSubspaceIdFromServer)
                    {
                        var targetTime = WarpSystem.Singleton.GetCurrentSubspaceTime();
                        var currentError = TimeSpan.FromSeconds(GetCurrentError()).TotalMilliseconds;
                        if (targetTime != 0 && Math.Abs(currentError) > MaxClockMsError)
                        {
                            if (Math.Abs(currentError) > MAX_CLOCK_SKEW)
                            {
                                Debug.LogWarning("Adjusted time from: "+ Planetarium.GetUniversalTime()+" to: "+targetTime+" due to error:"+currentError);
                                //TODO: This causes the throttle to reset when called.  This happens due to vessel unpacking resetting the throttle controls.
                                //TODO: Try to get Squad to change their code.
                                StepClock(targetTime);
                            }
                            else
                            {
                                SkewClock(currentError);
                            }
                        }
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

        public static void StepClock(double targetTick)
        {
            if (HighLogic.LoadedScene == GameScenes.LOADING)
            {
                Debug.Log("Skipping StepClock in loading screen");
                return;
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.fetch.activeVessel == null || !FlightGlobals.ready)
                {
                    Debug.Log("Skipping StepClock (active vessel is null or not ready)");
                    return;
                }
                try
                {
                    OrbitPhysicsManager.HoldVesselUnpack(5);
                }
                catch
                {
                    Debug.Log("Failed to hold vessel unpack");
                    return;
                }
                foreach (Vessel v in FlightGlobals.fetch.vessels)
                {
                    if (!v.packed)
                    {
                        if (v != FlightGlobals.fetch.activeVessel)
                        {
                            try
                            {
                                //For prelaunch vessels, we should not go on rails as this will reset the throttles and such, and 
                                if (v.situation != Vessel.Situations.PRELAUNCH)
                                {
                                    v.GoOnRails();
                                }
                            }
                            catch
                            {
                                Debug.Log("Error packing vessel " + v.id.ToString());
                            }
                        }
                        if (v == FlightGlobals.fetch.activeVessel)
                        {
                            if (SafeToStepClock(v, targetTick))
                            {
                                try
                                {
                                    v.GoOnRails();
                                }
                                catch
                                {
                                    Debug.Log("Error packing active vessel " + v.id.ToString());
                                }
                            }
                        }
                    }
                }
            }
            Planetarium.SetUniversalTime(targetTick);
        }

        private static bool SafeToStepClock(Vessel checkVessel, double targetTick)
        {
            switch (checkVessel.situation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.PRELAUNCH:
                case Vessel.Situations.SPLASHED:
                    //TODO: Fix.  We need to be able to adjust the clock on the ground, but then it resets the throttle position and does physics easing.
                    //TODO: For now, disable stepping the clock while landed.
                    return (checkVessel.srf_velocity.magnitude < 2);
                case Vessel.Situations.ORBITING:
                case Vessel.Situations.ESCAPING:
                    return true;
                case Vessel.Situations.SUB_ORBITAL:
                    double altitudeAtUT = checkVessel.orbit.getRelativePositionAtUT(targetTick).magnitude;
                    return (altitudeAtUT > checkVessel.mainBody.Radius + 10000 && checkVessel.altitude > 10000);
                default:
                    return false;
            }
        }

        private static void SkewClock(double currentError)
        {
            float timeWarpRate = (float)Math.Pow(2, -(currentError/1000f));
            if (timeWarpRate > MAX_CLOCK_RATE)
            {
                timeWarpRate = MAX_CLOCK_RATE;
            }
            if (timeWarpRate < MIN_CLOCK_RATE)
            {
                timeWarpRate = MIN_CLOCK_RATE;
            }

            //Set the physwarp rate
            Time.timeScale = timeWarpRate;
        }

        #endregion
    }
}