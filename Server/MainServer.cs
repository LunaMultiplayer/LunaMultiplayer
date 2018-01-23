using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Time;
using Server.Client;
using Server.Command;
using Server.Context;
using Server.Log;
using Server.Plugin;
using Server.Settings;
using Server.System;
using Server.System.VesselRelay;
using Server.Utilities;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class MainServer
    {
        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        public static void Main()
        {
            try
            {
                Console.Title = $"LMPServer {LmpVersioning.CurrentVersion}";
#if DEBUG
                Console.Title += " DEBUG";
#endif
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

                LunaLog.Normal($"Starting Luna Server version: {LmpVersioning.CurrentVersion}");

                Universe.CheckUniverse();
                LoadSettingsAndGroups();
                LmpPluginHandler.LoadPlugins();
                WarpSystem.Reset();
                ChatSystem.Reset();

                LunaLog.Normal($"Starting {GeneralSettings.SettingsStore.WarpMode} server on Port {GeneralSettings.SettingsStore.Port}... ");

                ServerContext.ServerRunning = true;
                ServerContext.LidgrenServer.SetupLidgrenServer();

                Task.Run(() => new CommandHandler().ThreadMain());
                Task.Run(() => new ClientMainThread().ThreadMain());

                Task.Run(() => ServerContext.LidgrenServer.StartReceiveingMessages());
                Task.Run(() => ServerContext.LidgrenServer.RegisterWithMasterServer());
                Task.Run(() => LogThread.RunLogThread());

                Task.Run(() => VesselRelaySystem.RelayOldVesselMessages());
                Task.Run(() => VesselUpdateRelaySystem.RelayToFarPlayers());
                Task.Run(() => VesselUpdateRelaySystem.RelayToMediumDistancePlayers());
                Task.Run(() => VesselUpdateRelaySystem.RelayToClosePlayers());
                Task.Run(() => VersionChecker.CheckForNewVersions());
                
                while (ServerContext.ServerStarting)
                    Thread.Sleep(500);

                LunaLog.Normal("All systems up and running. Поехали!");
                LmpPluginHandler.FireOnServerStart();

                QuitEvent.WaitOne();

                WarpSystem.SaveSubspacesToFile();
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
#if DEBUG
            DebugSettings.Singleton.Load();
            LunaTime.SimulatedMsTimeOffset = DebugSettings.SettingsStore.SimulatedMsTimeOffset;
#endif
        }

        //Gracefully shut down
        private static async void CatchExit(object sender, ConsoleCancelEventArgs args)
        {
            ServerContext.Shutdown();
            QuitEvent.Set();
            args.Cancel = true;
            await Task.Delay(5000);
        }

        /// <summary>
        /// Return the number of running instances.
        /// </summary>
        private static int GetRunningInstances() => Process.GetProcessesByName("LunaServer.exe").Length;
    }
}