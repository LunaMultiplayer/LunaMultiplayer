using System.Diagnostics;
using System.Collections.Generic;

namespace LunaClient.Utilities
{
    public class Timer
    {
        #region Fields

        private long _iterations = 0;
        private static readonly long WARM_UP_ITERATIONS = 60;
        private readonly Stopwatch _totalTimeWatch = new Stopwatch();
        private readonly Stopwatch _thisTimeWatch = new Stopwatch();
        private int _logInterval;
        private readonly double _tickLengthInMs = 1000d/Stopwatch.Frequency;
        private string _name;
        private static readonly Dictionary<string, Timer> TimerDictionary = new Dictionary<string, Timer>();

        #endregion

        #region Constructors
        private Timer(string name)
        {
            _name = name;
        }

        private static Timer GetTimer(string name, int logInterval)
        {
            Timer timer;
            if(!TimerDictionary.TryGetValue(name, out timer))
            {
                timer = new Timer(name) {_logInterval = logInterval};
                TimerDictionary.Add(name, timer);
            }
            return timer;
        }

        #endregion

        #region Methods

        public double GetMillisecondsThisTime()
        {
            var ticks = _thisTimeWatch.ElapsedTicks;
            return ((double)ticks) * _tickLengthInMs;
        }

        public static void Start(string name, int logInterval)
        {
#if !DEBUG
            return;
#endif
            var timer = GetTimer(name, logInterval);
            timer._iterations++;
            timer._thisTimeWatch.Reset();
            timer._thisTimeWatch.Start();
            if (timer._iterations > WARM_UP_ITERATIONS)
            {
                timer._totalTimeWatch.Start();
                
            }
        }

        public static void Stop(string name)
        {
#if !DEBUG
            return;
#endif
            var timer = GetTimer(name, 0);
            if (timer._iterations > WARM_UP_ITERATIONS)
            {
                timer._totalTimeWatch.Stop();
                timer._thisTimeWatch.Stop();
                var average = timer._totalTimeWatch.ElapsedMilliseconds / ((double)(timer._iterations - WARM_UP_ITERATIONS));

                var msThisTime = timer.GetMillisecondsThisTime();
                if (msThisTime > .01f && msThisTime > (average * 10))
                {
                    UnityEngine.Debug.Log($"Long run for {name}:{msThisTime}ms");
                }

                if (timer._logInterval != 0 && (timer._iterations - WARM_UP_ITERATIONS) % timer._logInterval == 0)
                {
                    //If we hit the log interval, log the average time.
                    UnityEngine.Debug.Log($"Average time per {name}:{average}ms");
                }
            }
        }

#endregion
    }
}
