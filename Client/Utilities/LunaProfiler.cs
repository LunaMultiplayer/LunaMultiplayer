using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LunaClient.Utilities
{
    //The lamest profiler in the world!
    public class LunaProfiler
    {
        public static Stopwatch LmpReferenceTime { get; } = new Stopwatch();
        public static ProfilerData FixedUpdateData { get; set; } = new ProfilerData();
        public static ProfilerData UpdateData { get; set; } = new ProfilerData();
        public static ProfilerData GuiData { get; set; } = new ProfilerData();

#if DEBUG
        public static ProfilerData Custom1 { get; set; } = new ProfilerData();
        public static ProfilerData Custom2 { get; set; } = new ProfilerData();
#endif
        public long Fps { get; set; }
    }

    public class ProfilerData
    {
        //Tick time is how long the method takes to run.
        private long TickMinTime { get; set; } = long.MaxValue;
        private long TickMaxTime { get; set; } = long.MinValue;
        private long TickTime { get; set; }
        private List<long> TickHistory { get; } = new List<long>();
        private long TickAverage { get; set; }

        //Delta time is how long it takes inbetween the method runs.
        private long DeltaMinTime { get; set; } = long.MaxValue;
        private long DeltaMaxTime { get; set; } = long.MinValue;
        private long LastDeltaTime { get; set; }
        private long DeltaTime { get; set; }
        private List<long> DeltaHistory { get; } = new List<long>();
        private long DeltaAverage { get; set; }

        public void ReportTime(long startClock)
        {
            var currentClock = LunaProfiler.LmpReferenceTime.ElapsedTicks;

            TickTime = currentClock - startClock;
            DeltaTime = startClock - LastDeltaTime;
            LastDeltaTime = currentClock;

            if (TickTime < TickMinTime)
                TickMinTime = TickTime;
            if (TickTime > TickMaxTime)
                TickMaxTime = TickTime;

            //Ignore the first delta as it will be incorrect on reset.
            if (DeltaHistory.Count != 0)
            {
                if (DeltaTime < DeltaMinTime)
                    DeltaMinTime = DeltaTime;
                if (DeltaTime > DeltaMaxTime)
                    DeltaMaxTime = DeltaTime;
            }

            TickHistory.Add(TickTime);
            if (TickHistory.Count > 300)
                TickHistory.RemoveAt(0);
            TickAverage = TickHistory.Sum();
            TickAverage /= TickHistory.Count;

            DeltaHistory.Add(DeltaTime);
            if (DeltaHistory.Count > 300)
                DeltaHistory.RemoveAt(0);
            DeltaAverage = DeltaHistory.Sum();
            DeltaAverage /= DeltaHistory.Count;
        }

        public override string ToString()
        {
            var tickMs = Math.Round(TickTime/(double) (Stopwatch.Frequency/1000), 3);
            var tickMinMs = Math.Round(TickMinTime/(double) (Stopwatch.Frequency/1000), 3);
            var tickMaxMs = Math.Round(TickMaxTime/(double) (Stopwatch.Frequency/1000), 3);
            var tickAverageMs = Math.Round(TickAverage/(double) (Stopwatch.Frequency/1000), 3);
            var deltaMs = Math.Round(DeltaTime/(double) (Stopwatch.Frequency/1000), 3);
            var deltaMinMs = Math.Round(DeltaMinTime/(double) (Stopwatch.Frequency/1000), 3);
            var deltaMaxMs = Math.Round(DeltaMaxTime/(double) (Stopwatch.Frequency/1000), 3);
            var deltaAverageMs = Math.Round(DeltaAverage/(double) (Stopwatch.Frequency/1000), 3);
            var returnString = "tick: " + tickMs + " (min/max/avg) " + tickMinMs + "/" + tickMaxMs + "/" + tickAverageMs +
                               "\n";
            returnString += "delta: " + deltaMs + " (min/max/avg) " + deltaMinMs + "/" + deltaMaxMs + "/" +
                            deltaAverageMs + "\n";

            return returnString;
        }
    }
}