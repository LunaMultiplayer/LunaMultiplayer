using System;
using System.Linq;
using System.Threading;
using LunaServer.Command.Command;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Plugin;
using LunaServer.Settings;
using LunaServer.System;

namespace LunaServer.Client
{
    public class ClientMainThread
    {
        public void ThreadMain()
        {
            try
            {
                WarpSystem.Reset();
                ChatSystem.Reset();

                while (ServerContext.ServerRunning)
                {
                    //Check timers
                    NukeCommand.CheckTimer();
                    Dekessler.CheckTimer();

                    LmpPluginHandler.FireOnUpdate(); //Run plugin update

                    Thread.Sleep(GeneralSettings.SettingsStore.MainTimeTick);
                }
            }
            catch (Exception e)
            {
                LunaLog.Error("Fatal error thrown, exception: " + e);
                new ShutDownCommand().Execute("Crashed!");
            }

            try
            {
                var disconnectTime = DateTime.UtcNow.Ticks;
                var sendingMessages = true;
                while (sendingMessages)
                {
                    if (DateTime.UtcNow.Ticks - disconnectTime > TimeSpan.FromSeconds(5).Ticks)
                    {
                        LunaLog.Debug($"Shutting down with {ServerContext.PlayerCount} Players, " +
                                      $"{ServerContext.Clients.Count} connected Clients");
                        break;
                    }
                    sendingMessages = ClientRetriever.GetAuthenticatedClients().Any(c => c.SendMessageQueue.Count > 0);

                    Thread.Sleep(GeneralSettings.SettingsStore.MainTimeTick);
                }
                ServerContext.LidgrenServer.ShutdownLidgrenServer();
            }
            catch (Exception e)
            {
                LunaLog.Fatal("Fatal error thrown during shutdown, exception: " + e);
                throw;
            }
        }
    }
}