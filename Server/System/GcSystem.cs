using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// It's a bad idea to run the GC, but somehow LMP is leaking somewhere. Until the leak is found, this can solve most issues...
    /// </summary>
    public class GcSystem
    {
        public static async void PerformGarbageCollection(CancellationToken token)
        {
            while (ServerContext.ServerRunning && IntervalSettings.SettingsStore.GcMinutesInterval != 0)
            {
                LunaLog.Debug("Performing a GarbageCollection");
                GC.Collect();
                try
                {
                    await Task.Delay((int)TimeSpan.FromMinutes(IntervalSettings.SettingsStore.GcMinutesInterval).TotalMilliseconds, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}