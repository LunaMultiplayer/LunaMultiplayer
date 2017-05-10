using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LunaClient.Utilities
{
    //The lamest profiler in the world!
    public class LunaProfiler
    {
        public static ProfilerData FixedUpdateData { get; } = new ProfilerData();
        public static ProfilerData UpdateData { get; } = new ProfilerData();
        public static ProfilerData GuiData { get; } = new ProfilerData();
    }

    public class ProfilerData
    {
        public static Stopwatch LmpReferenceTime { get; } = Stopwatch.StartNew();

        private long MinTime { get; set; } = long.MaxValue;
        private long MaxTime { get; set; } = long.MinValue;
        private long CurrentTime { get; set; }

        private List<long> History { get; } = new List<long>();
        private long Average => History.Sum() / History.Count;

        //Delta time is how long it takes inbetween the method runs.
        private long DeltaMinTime { get; set; } = long.MaxValue;
        private long DeltaMaxTime { get; set; } = long.MinValue;
        private long LastDeltaTime { get; set; }
        private long CurrentDeltaTime { get; set; }

        private List<long> DeltaHistory { get; } = new List<long>();
        private long DeltaAverage => DeltaHistory.Sum() / DeltaHistory.Count;

        public void Reset()
        {
            MinTime = long.MaxValue;
            MaxTime = long.MinValue;
            CurrentTime = 0;
            History.Clear();

            DeltaMinTime = long.MaxValue;
            DeltaMaxTime = long.MinValue;
            LastDeltaTime = 0;
            CurrentDeltaTime = 0;
            DeltaHistory.Clear();
        }

        public void ReportTime(long startClock)
        {
            var currentClock = LmpReferenceTime.ElapsedTicks;

            CurrentTime = currentClock - startClock;
            CurrentDeltaTime = startClock - LastDeltaTime;

            LastDeltaTime = currentClock;

            if (CurrentTime < MinTime)
                MinTime = CurrentTime;
            if (CurrentTime > MaxTime)
                MaxTime = CurrentTime;

            //Ignore the first delta as it will be incorrect on reset.
            if (DeltaHistory.Count != 0)
            {
                if (CurrentDeltaTime < DeltaMinTime)
                    DeltaMinTime = CurrentDeltaTime;
                if (CurrentDeltaTime > DeltaMaxTime)
                    DeltaMaxTime = CurrentDeltaTime;
            }

            History.Add(CurrentTime);
            if (History.Count > 300)
                History.RemoveAt(0);

            DeltaHistory.Add(CurrentDeltaTime);
            if (DeltaHistory.Count > 300)
                DeltaHistory.RemoveAt(0);
        }

        public override string ToString()
        {
            var returnString = $"Current: {Math.Round(TimeSpan.FromTicks(CurrentTime).TotalMilliseconds, 2)}ms (min/max/avg) " +
                               $"{Math.Round(TimeSpan.FromTicks(MinTime).TotalMilliseconds, 2)}/" +
                               $"{Math.Round(TimeSpan.FromTicks(MaxTime).TotalMilliseconds, 2)}/" +
                               $"{Math.Round(TimeSpan.FromTicks(Average).TotalMilliseconds, 2)}\n";

            //returnString += $"D: {CurrentDeltaTime}ms (min/max/avg) {DeltaMinTime}/{DeltaMaxTime}/{DeltaAverage}\n";
            
            return returnString;
        }
    }
}