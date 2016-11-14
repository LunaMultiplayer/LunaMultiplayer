using System.Diagnostics;
using System.Collections.Generic;

namespace LunaClient.Utilities
{
    public class Timer
    {
        #region Fields

        private long iterations = 0;
        private static readonly long WARM_UP_ITERATIONS = 60;
        private Stopwatch totalTimeWatch = new Stopwatch();
        private Stopwatch thisTimeWatch = new Stopwatch();
        public int logInterval { get; set; }
        private double tickLengthInMilliseconds = 1000d/Stopwatch.Frequency;
        private string name;
        private static Dictionary<string, Timer> timerDictionary = new Dictionary<string, Timer>();

        #endregion

        #region Constructors
        private Timer(string name)
        {
            this.name = name;
        }

        private static Timer getTimer(string name, int logInterval)
        {
            Timer timer;
            if(!timerDictionary.TryGetValue(name, out timer))
            {
                if(logInterval == 0)
                {
                    return null;
                }

                timer = new Timer(name);
                timer.logInterval = logInterval;
                timerDictionary.Add(name, timer);
            }
            return timer;
        }

        #endregion

        #region Methods

        public double getMillisecondsThisTime()
        {
            long ticks = thisTimeWatch.ElapsedTicks;
            return ((double)ticks) * tickLengthInMilliseconds;
        }

        public static void start(string name, int logInterval)
        {
            Timer timer = getTimer(name, logInterval);
            timer.iterations++;
            timer.thisTimeWatch.Reset();
            timer.thisTimeWatch.Start();
            if (timer.iterations > WARM_UP_ITERATIONS)
            {
                timer.totalTimeWatch.Start();
                
            }
        }

        public static void stop(string name)
        {
            Timer timer = getTimer(name, 0);
            if (timer.iterations > WARM_UP_ITERATIONS)
            {
                timer.totalTimeWatch.Stop();
                timer.thisTimeWatch.Stop();
                double average = timer.totalTimeWatch.ElapsedMilliseconds / ((double)(timer.iterations - WARM_UP_ITERATIONS));
                
                if(average != null)
                {
                    double msThisTime = timer.getMillisecondsThisTime();
                    if (msThisTime > (average*10))
                    {
                        UnityEngine.Debug.Log($"Long run for {name}:{msThisTime}ms");
                    }
                }

                if ((timer.iterations - WARM_UP_ITERATIONS) % timer.logInterval == 0)
                {
                    //Every 15 seconds of updates
                    UnityEngine.Debug.Log($"Average time per {name}:{average}ms");
                }
            }
        }

        #endregion
    }
}
