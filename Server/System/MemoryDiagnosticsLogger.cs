using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// Periodically samples a small set of process memory and GC counters and writes them to the
    /// main server log so operators (and the project) can tell the difference between a real
    /// managed leak and Server-GC working-set retention without attaching a profiler.
    ///
    /// Specifically, on each tick we record:
    ///   * Managed heap size (<see cref="GC.GetTotalMemory(bool)"/>) — the total bytes currently
    ///     reachable on the managed heap. If THIS climbs without bound, there is a real leak.
    ///   * Working set (<see cref="Environment.WorkingSet"/>) — physical pages currently mapped
    ///     to this process. Can grow well past the managed heap on Server GC and rarely shrinks
    ///     even after a Gen2 collection on Windows.
    ///   * Cumulative Gen0/Gen1/Gen2 collection counts so the operator can see whether
    ///     collections are happening at all between two adjacent samples.
    ///   * Allocation rate (delta of <see cref="GC.GetTotalAllocatedBytes(bool)"/> divided by
    ///     the elapsed sample window) — how hard the runtime is allocating. Steady, high
    ///     allocation rates point at allocation churn even when the heap is stable.
    ///
    /// Single responsibility: collect counters and emit one line per interval. Disabled by
    /// setting <see cref="Server.Settings.Definition.IntervalSettingsDefinition.MemoryDiagnosticsMinutesInterval"/>
    /// to <c>0</c>.
    /// </summary>
    public static class MemoryDiagnosticsLogger
    {
        public static async void LogMemoryDiagnostics(CancellationToken token)
        {
            // Capture the interval once. Settings can be reloaded at runtime in some setups, but
            // we deliberately don't react to live changes here: the goal is steady, comparable
            // samples across the whole observation window.
            var intervalMinutes = IntervalSettings.SettingsStore.MemoryDiagnosticsMinutesInterval;
            if (intervalMinutes <= 0) return;

            var intervalMs = (int)TimeSpan.FromMinutes(intervalMinutes).TotalMilliseconds;

            // Wait one full interval before the first sample so startup-time allocations (config
            // load, vessel/scenario load, plugin discovery, ...) don't dominate the first
            // alloc-rate number and mislead the reader.
            try
            {
                await Task.Delay(intervalMs, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            var previousAllocatedBytes = GC.GetTotalAllocatedBytes(precise: false);
            var previousSampleTicks = Environment.TickCount64;

            while (ServerContext.ServerRunning)
            {
                try
                {
                    var managedBytes = GC.GetTotalMemory(forceFullCollection: false);
                    var workingSetBytes = Environment.WorkingSet;
                    var gen0 = GC.CollectionCount(0);
                    var gen1 = GC.CollectionCount(1);
                    var gen2 = GC.CollectionCount(2);
                    var allocatedBytes = GC.GetTotalAllocatedBytes(precise: false);
                    var nowTicks = Environment.TickCount64;

                    // Compute allocation rate from the actual elapsed time, not the configured
                    // interval — Task.Delay is not exact and the difference matters when the
                    // operator changes the interval setting between runs.
                    var elapsedMs = Math.Max(1, nowTicks - previousSampleTicks);
                    var allocPerMinuteBytes = (allocatedBytes - previousAllocatedBytes) * 60_000L / elapsedMs;

                    LunaLog.Info(
                        $"[MemDiag] Managed heap: {ToMb(managedBytes)} MB | " +
                        $"Working set: {ToMb(workingSetBytes)} MB | " +
                        $"Gen0/1/2 collections: {gen0}/{gen1}/{gen2} | " +
                        $"Alloc rate: {ToMb(allocPerMinuteBytes)} MB/min");

                    previousAllocatedBytes = allocatedBytes;
                    previousSampleTicks = nowTicks;

                    await Task.Delay(intervalMs, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    // The diagnostics loop must never crash the server, but if it fails repeatedly
                    // we want the operator to know. Log once and continue with the same cadence.
                    LunaLog.Error($"Memory diagnostics logger failed: {e.Message}");
                    try
                    {
                        await Task.Delay(intervalMs, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        // Integer division so the log line stays compact and human-skimmable; precision past 1 MB
        // is irrelevant for separating "stable heap" from "climbing heap" trends.
        private static long ToMb(long bytes) => bytes / 1024 / 1024;
    }
}
