using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpCommon.Time;
using System;
using UnityEngine;

namespace LmpClient.Systems.TimeSyncer
{
    /// <summary>
    /// This system syncs the game time with the UTC time.
    /// Your game time may shift, for example when you computer hangs or there are lots of vessels etc.
    /// Those "micro hangs" should be corrected so all the players are in the same time as the UTC
    /// </summary>
    public class TimeSyncerSystem : System<TimeSyncerSystem>
    {
        #region Fields & properties

        #region Public

        public TimerSyncerEvents TimerSyncerEvents { get; } = new TimerSyncerEvents();

        public static long ServerStartTime { get; set; }

        private static double _universalTime;

        /// <summary>
        /// Thread safe way of accessing Planetarium.GetUniversalTime()
        /// </summary>
        public static double UniversalTime
        {
            get => MainSystem.IsUnityThread ? Planetarium.GetUniversalTime() : _universalTime;
            private set => _universalTime = value;
        }

        /// <summary>
        /// Gets the server clock in seconds.
        /// </summary>
        public static double ServerClockSec => TimeUtil.TicksToSeconds(LunaNetworkTime.UtcNow.Ticks - ServerStartTime);

        /// <summary>
        /// Gets the current time error between the server time and the game time
        /// </summary>
        public static double CurrentErrorSec => UniversalTime - WarpSystem.Singleton.CurrentSubspaceTime;

        #endregion

        #region Constants

        /// <summary>
        /// If the time between UTC and game is greater than this, the game time will be fixed using the phisics clock (makes game go faster/slower)
        /// </summary>
        private const int MinPhisicsClockMsError = 25;
        /// <summary>
        /// Minimum speed that the game can go
        /// </summary>
        private const float MinPhisicsClockRate = 0.85f;
        /// <summary>
        /// Max speed that the game can go. If you put this number too high the game will lag a lot.
        /// </summary>
        private const float MaxPhisicsClockRate = 1.20f;
        /// <summary>
        /// Limit at which we won't fix the time with the GAME timescale
        /// </summary>
        private const int MaxPhisicsClockMsError = 3500;

        #endregion

        #region Private

        private static bool CurrentlyWarping => WarpSystem.Singleton.CurrentSubspace == -1;

        private static bool CanSyncTime
        {
            get
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
        }

        #endregion

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(TimeSyncerSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();

            //Refresh it at TimingManager.TimingStage.Precalc so it's updated JUST after KSP updates it's time
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Precalc, SetGameTime);

            //Uncomment to check if the TimingManager.TimingStage.Precalc is the correct time. All the operations should give "0"
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.ObscenelyEarly, ()=> LunaLog.Log($"ObscenelyEarly {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Early, () => LunaLog.Log($"Early {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Precalc, () => LunaLog.Log($"Precalc {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Earlyish, () => LunaLog.Log($"Earlyish {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Normal, () => LunaLog.Log($"Normal {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.FashionablyLate, () => LunaLog.Log($"FashionablyLate {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.FlightIntegrator, () => LunaLog.Log($"FlightIntegrator {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Late, () => LunaLog.Log($"Late {Planetarium.GetUniversalTime() - UniversalTime}"));
            //TimingManager.FixedUpdateAdd(TimingManager.TimingStage.BetterLateThanNever, () => LunaLog.Log($"BetterLateThanNever {Planetarium.GetUniversalTime() - UniversalTime}"));

            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Precalc, SyncTimeScale);

            SpectateEvent.onStartSpectating.Add(TimerSyncerEvents.OnStartSpectating);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.Precalc, SetGameTime);
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.Precalc, SyncTimeScale);

            SpectateEvent.onStartSpectating.Remove(TimerSyncerEvents.OnStartSpectating);
            ServerStartTime = 0;
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Routine that checks our time against the server time and adjust it if needed.
        /// We only adjust the GAME time. And we will only do if the time error is between <see cref="MinPhisicsClockMsError"/> and <see cref="MaxPhisicsClockMsError"/>
        /// We cannot do it with more than <see cref="MaxPhisicsClockMsError"/> as then we would need a lot of time to catch up with the time error.
        /// For greater errors we just fix the time with the StepClock
        /// </summary>
        private void SyncTimeScale()
        {
            if (Enabled && !CurrentlyWarping && CanSyncTime && !WarpSystem.Singleton.WaitingSubspaceIdFromServer)
            {
                var targetTime = WarpSystem.Singleton.CurrentSubspaceTime;
                var currentError = TimeUtil.SecondsToMilliseconds(CurrentErrorSec);

                if (Math.Abs(currentError) < MinPhisicsClockMsError)
                {
                    Time.timeScale = 1;
                }
                if (Math.Abs(currentError) > MinPhisicsClockMsError && Math.Abs(currentError) < MaxPhisicsClockMsError)
                {
                    //Time error is not so big so we can fix it adjusting the physics time
                    SkewClock();
                }
                else if (Math.Abs(currentError) > MaxPhisicsClockMsError)
                {
                    LunaLog.LogWarning($"[LMP] Adjusted time from: {UniversalTime} to: {targetTime} due to error: {currentError}");
                    ClockHandler.StepClock(targetTime);
                }
            }
        }

        #endregion

        #region FixedUpdateMethods

        /// <summary>
        /// Updates the UniversalTime with the game time on every fixed update
        /// </summary>
        private static void SetGameTime()
        {
            UniversalTime = Planetarium.GetUniversalTime();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Forces a time sync against the server time
        /// </summary>
        public void ForceTimeSync()
        {
            if (Enabled && !CurrentlyWarping && CanSyncTime && !WarpSystem.Singleton.WaitingSubspaceIdFromServer)
            {
                var targetTime = WarpSystem.Singleton.CurrentSubspaceTime;
                var currentError = TimeUtil.SecondsToMilliseconds(CurrentErrorSec);

                LunaLog.LogWarning($"FORCING a time sync from: {UniversalTime} to: {targetTime}. Error: {currentError}");
                ClockHandler.StepClock(targetTime);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Here we adjust the GAME timescale and make the game goes faster or slower
        /// </summary>
        private static void SkewClock()
        {
            var timeWarpRate = (float)Math.Pow(2, -CurrentErrorSec);

            //Set the physwarp rate between the max and min allowed
            Time.timeScale = Mathf.Clamp(timeWarpRate, MinPhisicsClockRate, MaxPhisicsClockRate);
        }

        #endregion
    }
}
