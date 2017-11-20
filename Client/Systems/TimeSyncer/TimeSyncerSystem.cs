using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using System;
using UnityEngine;

namespace LunaClient.Systems.TimeSyncer
{
    /// <summary>
    /// This system syncs the game time with the UTC time.
    /// Your game time may shift, for example when you computer hangs or there are lots of vessels etc.
    /// Those "micro hangs" should be corrected so all the players are in the same time as the UTC
    /// </summary>
    public class TimeSyncerSystem : Base.System
    {
        #region Fields & properties

        #region Public

        public static long ServerStartTime { get; set; }

        /// <summary>
        /// Gets the server clock as total secconds and applying the network latency to the result.
        /// </summary>
        public static double ServerClockSec => TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ServerStartTime + NetworkStatistics.TimeOffset).TotalSeconds;

        /// <summary>
        /// Gets the current time error between the server time and the game time
        /// </summary>
        public static double CurrentErrorSec => Planetarium.GetUniversalTime() - SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime();

        #endregion

        #region Constants

        /// <summary>
        /// If the time between UTC and game is greater than this, the game time 
        /// </summary>
        private const int MaxPhisicsClockMsError = 250;
        /// <summary>
        /// Minimum speed that the game can go
        /// </summary>
        private const float MinPhisicsClockRate = 0.5f;
        /// <summary>
        /// Max speed that the game can go
        /// </summary>
        private const float MaxPhisicsClockRate = 1.5f;
        /// <summary>
        /// Limit at wich we won't fix the time with the GAME timescale
        /// </summary>
        private const int PhisicsClockLimitMs = 15000;
        /// <summary>
        /// If the time difference is greater than this, the game will set a new time as a global
        /// </summary>
        private const int MaxClockErrorMs = 15000;

        #endregion

        #region Private

        private static bool CurrentlyWarping => SystemsContainer.Get<WarpSystem>().CurrentSubspace == -1;
        
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

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, SyncTimeScale));
            SetupRoutine(new RoutineDefinition(15000, RoutineExecution.Update, SyncTime));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
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
            if (Enabled && !CurrentlyWarping && CanSyncTime && !SystemsContainer.Get<WarpSystem>().WaitingSubspaceIdFromServer)
            {
                var targetTime = (int)SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime();
                var currentError = TimeSpan.FromSeconds(CurrentErrorSec).TotalMilliseconds;
                if (targetTime != 0 && Math.Abs(currentError) > MaxPhisicsClockMsError && Math.Abs(currentError) < PhisicsClockLimitMs)
                {
                    //Time error is not so big so we can fix it adjusting the phisics time
                    SkewClock();
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
            if (Enabled && !CurrentlyWarping && CanSyncTime && !SystemsContainer.Get<WarpSystem>().WaitingSubspaceIdFromServer)
            {
                var targetTime = (int)SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime();
                var currentError = TimeSpan.FromSeconds(CurrentErrorSec).TotalMilliseconds;
                if (targetTime != 0 && Math.Abs(currentError) > MaxClockErrorMs)
                {
                    LunaLog.LogWarning($"[LMP] Adjusted time from: {Planetarium.GetUniversalTime()} to: {targetTime} due to error:{currentError}");
                    ClockHandler.StepClock(targetTime);
                }
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