using LunaCommon.Time;
using Server.Command.Command;
using Server.Context;
using Server.Lidgren;
using Server.Log;
using Server.Plugin;
using Server.Settings.Structures;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Client
{
    public class ClientMainThread
    {
        public async void ThreadMain()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    //Check timers
                    NukeCommand.CheckTimer();
                    DekesslerCommand.CheckTimer();

                    LmpPluginHandler.FireOnUpdate(); //Run plugin update

                    await Task.Delay(GeneralSettings.SettingsStore.MainTimeTick);
                }
            }
            catch (Exception e)
            {
                LunaLog.Error($"Fatal error thrown, exception: {e}");
                ServerContext.Shutdown("Fatal error server side");
            }

            try
            {
                var disconnectTime = LunaTime.UtcNow.Ticks;
                var sendingMessages = true;
                while (sendingMessages)
                {
                    if (LunaTime.UtcNow.Ticks - disconnectTime > TimeSpan.FromSeconds(5).Ticks)
                    {
                        LunaLog.Debug($"Shutting down with {ServerContext.PlayerCount} Players, " +
                                      $"{ServerContext.Clients.Count} connected Clients");
                        break;
                    }
                    sendingMessages = ClientRetriever.GetAuthenticatedClients().Any(c => c.SendMessageQueue.Count > 0);

                    await Task.Delay(GeneralSettings.SettingsStore.MainTimeTick);
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
