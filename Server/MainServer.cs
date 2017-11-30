using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Time;
using LunaServer.Client;
using LunaServer.Command;
using LunaServer.Command.Command;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Plugin;
using LunaServer.Settings;
using LunaServer.System;
using LunaServer.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LunaServer
{
    public class MainServer
    {
        public static void Main()
        {
            try
            {
                Console.Title = $"LMPServer {Common.CurrentVersion}";
                Console.OutputEncoding = Encoding.Unicode;
                ServerContext.StartTime = LunaTime.UtcNow.Ticks;

                //We cannot run more than 6 instances ofd servers + clients as otherwise the sync time will fail (30 seconds / 5 seconds = 6) but we use 3 for safety
                if (GetRunningInstances() > 3)
                    throw new HandledException("Cannot run more than 3 servers at a time!");
                
                //Start the server clock
                ServerContext.ServerClock.Start();

                //Set the last player activity time to server start
                ServerContext.LastPlayerActivity = ServerContext.ServerClock.ElapsedMilliseconds;

                //Register the ctrl+c event
                Console.CancelKeyPress += CatchExit;
                ServerContext.ServerStarting = true;

                //Set day for log change
                ServerContext.Day = LunaTime.Now.Day;
                
                LunaLog.Normal($"Starting Luna Server version: {Common.CurrentVersion}");

                Universe.CheckUniverse();
                LmpPluginHandler.LoadPlugins();
                LoadSettingsAndGroups();

                LunaLog.Normal($"Starting {GeneralSettings.SettingsStore.WarpMode} server on Port {GeneralSettings.SettingsStore.Port}... ");

                ServerContext.ServerRunning = true;
                ServerContext.LidgrenServer.SetupLidgrenServer();

                var commandThread = Task.Run(() => new CommandHandler().ThreadMain());
                var clientThread = Task.Run(() => new ClientMainThread().ThreadMain());

                var receiveThread = Task.Run(() => ServerContext.LidgrenServer.StartReceiveingMessages());
                var registerThread = Task.Run(() => ServerContext.LidgrenServer.RegisterWithMasterServer());
                var logThread = Task.Run(() => LogThread.RunLogThread());

                var vesselRelayThread = Task.Run(() => VesselRelaySystem.RelayOldVesselMessages());
                var vesselRelayFarThread = Task.Run(() => VesselUpdateRelaySystem.RelayToFarPlayers());
                var vesselRelayMediumThread = Task.Run(() => VesselUpdateRelaySystem.RelayToMediumDistancePlayers());
                var vesselRelayCloseThread = Task.Run(() => VesselUpdateRelaySystem.RelayToClosePlayers());

                Thread.Sleep(1000);

                while (ServerContext.ServerStarting)
                    Thread.Sleep(500);

                LunaLog.Normal("All systems up and running. Поехали!");
                LmpPluginHandler.FireOnServerStart();

                receiveThread.Wait();
                registerThread.Wait();
                commandThread.Wait();
                clientThread.Wait();
                logThread.Wait();

                vesselRelayThread.Wait();
                vesselRelayFarThread.Wait();
                vesselRelayMediumThread.Wait();
                vesselRelayCloseThread.Wait();

                LmpPluginHandler.FireOnServerStop();

                LunaLog.Normal("Goodbye and thanks for all the fish!");
            }
            catch (Exception e)
            {
                if (e is HandledException)
                    LunaLog.Fatal(e.Message);
                else
                    LunaLog.Fatal($"Error in main server thread, Exception: {e}");
                Console.ReadLine(); //Avoid closing automatically
            }
        }

        private static void LoadSettingsAndGroups()
        {
            LunaLog.Debug("Loading groups...");
            GroupSystem.LoadGroups();
            LunaLog.Debug("Loading settings...");
            GeneralSettings.Singleton.Load();
            if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom)
            {
                LunaLog.Debug("Loading gameplay settings...");
                GameplaySettings.Singleton.Load();
            }
        }

        //Gracefully shut down
        private static void CatchExit(object sender, ConsoleCancelEventArgs args)
        {
            new ShutDownCommand().Execute("Caught Ctrl+C");
            Thread.Sleep(5000);
        }

        /// <summary>
        /// Return the number of running instances.
        /// </summary>
        private static int GetRunningInstances() => Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Length;
    }
}