using System;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public enum CustomizationResult { TooEarly, IgnoreSend, IgnoreReceive, Ok }

    public class PartSyncUpdateEntry
    {
        private DateTime _lastUpdateTime;
        private readonly int _intervalInMs;

        public bool IntervalIsOk() => DateTime.Now - _lastUpdateTime > TimeSpan.FromMilliseconds(_intervalInMs);

        public PartSyncUpdateEntry(int intervalMs)
        {
            _intervalInMs = intervalMs;
            _lastUpdateTime = DateTime.Now;
        }

        public void Update()
        {
            _lastUpdateTime = DateTime.Now;
        }
    }
}
