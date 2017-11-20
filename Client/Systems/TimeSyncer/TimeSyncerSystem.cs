using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using System;
using UnityEngine;

namespace LunaClient.Systems.TimeSyncer
{
    /// <summary>
    /// This system syncs the server time with your time.
    /// Your game time may shift, for example when you computer hangs, when there are lots of vessels etc.
    /// Those "micro hangs" should be corrected so all the players are in the same time as the server
    /// </summary>
    public class TimeSyncerSystem : Base.System
    {
        #region Fields & properties

        #region Public

        public static long ServerStartTime { get; set; }

        #endregion

        #region Constants

        private const int MaxClockMsError = 100;
        private const float MinClockRate = 0.3f;
        private const float MaxClockRate = 1.5f;
        private const int MaxClockSkew = 5000;

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
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.ClockSetMsInterval, RoutineExecution.Update, SyncTime));
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
        /// </summary>
        /// <returns></returns>
        private void SyncTime()
        {
            if (Enabled && !CurrentlyWarping && CanSyncTime && !SystemsContainer.Get<WarpSystem>().WaitingSubspaceIdFromServer)
            {
                var targetTime = (int)SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime();
                var currentError = TimeSpan.FromSeconds(GetCurrentError()).TotalMilliseconds;
                if (targetTime != 0 && Math.Abs(currentError) > MaxClockMsError)
                {
                    if (Math.Abs(currentError) > MaxClockSkew)
                    {
                        LunaLog.LogWarning($"[LMP] Adjusted time from: {Planetarium.GetUniversalTime()} to: {targetTime} due to error:{currentError}");
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
        /// Gets the server clock as total secconds and applying the network latency to the result.
        /// </summary>
        public double GetServerClock() => TimeSpan.FromTicks(DateTime.UtcNow.Ticks - ServerStartTime + NetworkStatistics.TimeOffset).TotalSeconds;

        /// <summary>
        /// Gets the current time error between the server time and the game time
        /// </summary>
        public double GetCurrentError() => Planetarium.GetUniversalTime() - SystemsContainer.Get<WarpSystem>().GetCurrentSubspaceTime();

        #endregion

        #region Private methods

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