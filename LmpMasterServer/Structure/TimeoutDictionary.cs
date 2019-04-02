using System.Collections.Concurrent;
using System.Timers;

namespace LmpMasterServer.Structure
{
    public class TimeoutConcurrentDictionary<TKey, TValue>
    {
        private readonly double _timeutInMs;

        private class TimedValue
        {
            private readonly Timer _timer;
            public readonly TValue Value;
            private readonly TKey _key;

            public TimedValue(TKey key, TValue value, double timeout)
            {
                _key = key;
                Value = value;
                _timer = new Timer(timeout);
                _timer.Elapsed += Elapsed_Event;
                _timer.Start();
            }

            private void Elapsed_Event(object sender, ElapsedEventArgs e)
            {
                _timer.Elapsed -= Elapsed_Event;
                Dictionary.TryRemove(_key, out _);
            }
        }

        private static readonly ConcurrentDictionary<TKey, TimedValue> Dictionary = new ConcurrentDictionary<TKey, TimedValue>();

        public TimeoutConcurrentDictionary(double timeoutInMs) => _timeutInMs = timeoutInMs;

        public bool TryAdd(TKey key, TValue value)
        {
            return Dictionary.TryAdd(key, new TimedValue(key, value, _timeutInMs));
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (Dictionary.TryGetValue(key, out var timedVal))
            {
                value = timedVal.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }
    }
}
