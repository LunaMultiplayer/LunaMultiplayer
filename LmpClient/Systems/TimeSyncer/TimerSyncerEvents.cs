using LmpClient.Base;

namespace LmpClient.Systems.TimeSyncer
{
    public class TimerSyncerEvents:SubSystem<TimeSyncerSystem>
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
