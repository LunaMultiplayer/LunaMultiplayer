using LmpCommon.Time;
using Server.Command.Command;
using Server.Context;
using Server.Log;
using Server.Plugin;
using Server.Server;
using Server.Settings.Structures;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Client
{
    public class ClientMainThread
    {
        public static async Task ThreadMainAsync()
        {
            // PeriodicTimer instead of per-tick Task.Delay. At idle this loop runs ~200 times/sec
            // and the Task.Delay churn was, together with the Lidgren receive loop, the dominant
            // source of the ~2 MB/min idle managed-heap allocation rate observed in [MemDiag].
            // PeriodicTimer allocates exactly once and reuses an internal ValueTask each tick.
            //
            // Period is captured at start time and clamped to >=1 ms because PeriodicTimer rejects
            // a non-positive period; this matches the "Keep this value low but at least above 2ms"
            // guidance on MainTimeTick without crashing on misconfiguration.
            var tickMs = Math.Max(1, IntervalSettings.SettingsStore.MainTimeTick);
            var shutdownToken = MainServer.CancellationTokenSrc.Token;

            try
            {
                using var mainTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(tickMs));
                while (ServerContext.ServerRunning)
                {
                    //Check timers
                    NukeCommand.CheckTimer();
                    DekesslerCommand.CheckTimer();

                    LmpPluginHandler.FireOnUpdate(); //Run plugin update

                    if (!await mainTimer.WaitForNextTickAsync(shutdownToken))
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown path — the cancellation token was tripped while we were waiting
                // for the next tick. Fall through to the drain loop below.
            }
            catch (Exception e)
            {
                LunaLog.Error($"Fatal error thrown, exception: {e}");
                ServerContext.Shutdown("Fatal error server side");
            }

            try
            {
                // Drain any pending outgoing messages before the Lidgren server shuts down. The
                // shutdown token is *deliberately* not passed to WaitForNextTickAsync here: by the
                // time we reach this loop the global cancellation has already fired, and honoring
                // it would skip the drain and disconnect clients with messages still queued.
                using var drainTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(tickMs));
                var disconnectTime = LunaNetworkTime.UtcNow.Ticks;
                var sendingMessages = true;
                while (sendingMessages)
                {
                    if (LunaNetworkTime.UtcNow.Ticks - disconnectTime > TimeSpan.FromSeconds(5).Ticks)
                    {
                        LunaLog.Debug($"Shutting down with {ServerContext.PlayerCount} Players, " +
                                      $"{ServerContext.Clients.Count} connected Clients");
                        break;
                    }
                    sendingMessages = ClientRetriever.GetAuthenticatedClients().Any(c => c.SendMessageQueue.Count > 0);

                    if (!await drainTimer.WaitForNextTickAsync(CancellationToken.None))
                        break;
                }
                LidgrenServer.ShutdownLidgrenServer();
            }
            catch (Exception e)
            {
                LunaLog.Fatal($"Fatal error thrown during shutdown, exception: {e}");
                throw;
            }
        }
    }
}
