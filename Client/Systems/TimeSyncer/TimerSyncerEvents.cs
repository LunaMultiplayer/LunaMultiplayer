using LunaClient.Base;

namespace LunaClient.Systems.TimeSyncer
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
