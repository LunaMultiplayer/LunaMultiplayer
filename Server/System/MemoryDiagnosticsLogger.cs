using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// Periodically logs heap size, working set, GC counts, and allocation rate.
    /// Intended to distinguish true managed growth from expected Server-GC working-set behavior.
    /// Disabled when the diagnostics interval is set to 0.
    /// </summary>
    public static class MemoryDiagnosticsLogger
    {
        public static async Task LogMemoryDiagnosticsAsync(CancellationToken token)
        {
            // Capture once for stable, comparable samples.
            var intervalMinutes = IntervalSettings.SettingsStore.MemoryDiagnosticsMinutesInterval;
            if (intervalMinutes <= 0) return;

            var intervalMs = (int)TimeSpan.FromMinutes(intervalMinutes).TotalMilliseconds;

            // Delay first sample so startup allocations do not skew the baseline.
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

                    // Use actual elapsed time rather than nominal interval.
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
                    // Best-effort diagnostics: report failures and continue.
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

        // Integer MB keeps the line compact and trend-oriented.
        private static long ToMb(long bytes) => bytes / 1024 / 1024;
    }
}
