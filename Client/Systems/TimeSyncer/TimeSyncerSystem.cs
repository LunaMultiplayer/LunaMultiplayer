using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using LunaCommon.Time;
using System;
using UnityEngine;

namespace LunaClient.Systems.TimeSyncer
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

        /// <summary>
        /// Gets the server clock in seconds.
        /// </summary>
        public static double ServerClockSec => TimeUtil.TicksToSeconds(LunaTime.UtcNow.Ticks - ServerStartTime);

        /// <summary>
        /// Gets the current time error between the server time and the game time
        /// </summary>
        public static double CurrentErrorSec => Planetarium.GetUniversalTime() - WarpSystem.Singleton.CurrentSubspaceTime;

        #endregion

        #region Constants

        /// <summary>
        /// If the time between UTC and game is greater than this, the game time will be fixed using the phisics clock (makes game go faster/slower)
        /// </summary>
        private const int MaxPhisicsClockMsError = 25;
        /// <summary>
        /// Minimum speed that the game can go
        /// </summary>
        private const float MinPhisicsClockRate = 0.85f;
        /// <summary>
        /// Max speed that the game can go. If you put this number too high the game will lag a lot.
        /// </summary>
        private const float MaxPhisicsClockRate = 1.20f;
        /// <summary>
        /// Limit at wich we won't fix the time with the GAME timescale
        /// </summary>
        private const int PhisicsClockLimitMs = 15000;
        /// <summary>
        /// If the time difference is greater than this, the game will set a new time as a global
        /// </summary>
        private const int MaxClockErrorMs = 15000;
        /// <summary>
        /// Limit at wich we won't fix the time with the GAME timescale when spectating
        /// </summary>
        private const int SpectatingPhisicsClockLimitMs = 2500;
        /// <summary>
        /// If the time difference is greater than this, the game will set a new time as a global when spectating
        /// </summary>
        private const int MaxSpectatingClockErrorMs = 2500;

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
            SpectateEvent.onStartSpectating.Add(TimerSyncerEvents.OnStartSpectating);
            SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, SyncTimeScale));
            SetupRoutine(new RoutineDefinition(15000, RoutineExecution.Update, SyncTime));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            SpectateEvent.onStartSpectating.Remove(TimerSyncerEvents.OnStartSpectating);
            ServerStartTime = 0;
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Routine that checks our time against the server time and adjust it if needed.
        /// We only adjust the GAME time. And we will only do if the time error is between 100ms to 5000ms
        /// We cannot do it with more than 15000 as then we would need a lot of time to catch up with the time error.
        /// For greater errors we just fix the time with the SyncTime routine that uses the planetarium
        /// </summary>
        private void SyncTimeScale()
        {
            if (Enabled && !CurrentlyWarping && CanSyncTime && !WarpSystem.Singleton.WaitingSubspaceIdFromServer)
            {
                var targetTime = WarpSystem.Singleton.CurrentSubspaceTime;
                if (targetTime > 0)
                {
                    var currentError = TimeUtil.SecondsToMilliseconds(CurrentErrorSec);
                    if (Math.Abs(currentError) > MaxPhisicsClockMsError && Math.Abs(currentError) < (VesselCommon.IsSpectating ? SpectatingPhisicsClockLimitMs : PhisicsClockLimitMs))
                    {
                        //Time error is not so big so we can fix it adjusting the physics time
                        SkewClock();
                    }
                }
            }
        }

        /// <summary>
        /// Routine that checks our time against the server time and adjust it if needed.
        /// If the error is too big this routine will adjust the clock with the planetarium instead of accelerating / decreasing the game time
        /// </summary>
        /// <returns></returns>
        private void SyncTime()
        {
            if (Enabled && !CurrentlyWarping && CanSyncTime && !WarpSystem.Singleton.WaitingSubspaceIdFromServer)
            {
                var targetTime = WarpSystem.Singleton.CurrentSubspaceTime;
                var currentError = TimeUtil.SecondsToMilliseconds(CurrentErrorSec);

                if (targetTime > 0 && Math.Abs(currentError) > (VesselCommon.IsSpectating ? MaxSpectatingClockErrorMs : MaxClockErrorMs))
                {
                    LunaLog.LogWarning($"[LMP] Adjusted time from: {Planetarium.GetUniversalTime()} to: {targetTime} due to error:{currentError}");
                    ClockHandler.StepClock(targetTime);
                }
            }
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

                LunaLog.LogWarning($"FORCING a time sync from: {Planetarium.GetUniversalTime()} to: {targetTime}. Error:{currentError}");
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
