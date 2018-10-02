using LmpClient.Base;

namespace LmpClient.Systems.TimeSync
{
    public class TimerSyncEvents : SubSystem<TimeSyncSystem>
    {
        /// <summary>
        /// When start to spectate force a sync with server
        /// </summary>
        public void OnStartSpectating()
        {
            System.ForceTimeSync();
        }
    }
}
