using System;
using System.Diagnostics;
using LunaClient.Utilities;

namespace LunaClient.Base
{
    public enum RoutineExecution
    {
        Update,
        FixedUpdate
    }

    public class RoutineDefinition
    {
        private readonly ProfilerData _profiler = new ProfilerData();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public string Name => Method.Method.Name;
        public int IntervalInMs { get; set; }
        public Action Method { private get; set; }
        public RoutineExecution Execution { get; set; }

        #region Constructors

        private RoutineDefinition()
        {
            _stopwatch.Start();
        }

        public RoutineDefinition(int intervalInMs, RoutineExecution execution, Action method) : this()
        {
            IntervalInMs = intervalInMs;
            Execution = execution;
            Method = method;
        }

        #endregion

        public void RunRoutine()
        {
            if (IntervalInMs <= 0 || _stopwatch.ElapsedMilliseconds > IntervalInMs)
            {
                var startClock = ProfilerData.LmpReferenceTime.ElapsedTicks;

                Method.Invoke();

                _profiler.ReportTime(startClock);
                _stopwatch.Reset();
                _stopwatch.Start();
            }
        }
    }
}
