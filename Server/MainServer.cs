using LunaCommon;
using LunaCommon.Enums;
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LunaServer
{
    public class MainServer
    {
        private static long _lastLogExpiredCheck;
        private static long _lastDayCheck;

        public static void Main()
        {
            try
            {
                ServerContext.StartTime = DateTime.UtcNow.Ticks;

                //Start the server clock
                ServerContext.ServerClock.Start();

                //Set the last player activity time to server start
                ServerContext.LastPlayerActivity = ServerContext.ServerClock.ElapsedMilliseconds;

                //Register the ctrl+c event
                Console.CancelKeyPress += CatchExit;
                ServerContext.ServerStarting = true;

                LunaLog.Debug("Loading settings...");
                if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom)
                {
                    GameplaySettings.Singleton.Load();
                }

                //Set day for log change
                ServerContext.Day = DateTime.Now.Day;

                //Load plugins
                LmpPluginHandler.LoadPlugins();

                Console.Title = $"LMPServer v{VersionInfo.VersionNumber}";

                while (ServerContext.ServerStarting || ServerContext.ServerRestarting)
                {
                    if (ServerContext.ServerRestarting)
                    {
                        LunaLog.Debug("Reloading settings...");
                        GeneralSettings.Singleton.Load();
                        if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom)
                        {
                            LunaLog.Debug("Reloading gameplay settings...");
                            GameplaySettings.Singleton.Load();
                        }
                    }

                    ServerContext.ServerRestarting = false;
                    LunaLog.Normal($"Starting Luna Server v{VersionInfo.FullVersionNumber}");

                    if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom)
                    {
                        //Generate the config file by accessing the object.
                        LunaLog.Debug("Loading gameplay settings...");
                        GameplaySettings.Singleton.Load();
                    }

                    //Load universe
                    LunaLog.Normal("Loading universe... ");
                    Universe.CheckUniverse();

                    LunaLog.Normal($"Starting {GeneralSettings.SettingsStore.WarpMode} server on Port {GeneralSettings.SettingsStore.Port}... ");

                    ServerContext.ServerRunning = true;

                    ServerContext.LidgrenServer.SetupLidgrenServer();

                    var commandThread = Task.Run(() => new CommandHandler().ThreadMain());
                    var clientThread = Task.Run(() => new ClientMainThread().ThreadMain());

                    Task.Run(() => ServerContext.LidgrenServer.StartReceiveingMessages());
                    Task.Run(() => ServerContext.LidgrenServer.RegisterWithMasterServer());

                    var vesselRelayThread = Task.Run(() => VesselRelaySystem.RelayOldVesselMessages());

                    var vesselRelayFarThread = Task.Run(() => VesselUpdateRelaySystem.RelayToFarPlayers());
                    var vesselRelayMediumThread = Task.Run(() => VesselUpdateRelaySystem.RelayToMediumDistancePlayers());
                    var vesselRelayCloseThread = Task.Run(() => VesselUpdateRelaySystem.RelayToClosePlayers());

                    while (ServerContext.ServerStarting)
                        Thread.Sleep(500);

                    LunaLog.Normal("Ready!");
                    LmpPluginHandler.FireOnServerStart();
                    while (ServerContext.ServerRunning)
                    {
                        //Run the log expire function every 10 minutes
                        if (ServerContext.ServerClock.ElapsedMilliseconds - _lastLogExpiredCheck > 600000)
                        {
                            _lastLogExpiredCheck = ServerContext.ServerClock.ElapsedMilliseconds;
                            LogExpire.ExpireLogs();
                        }

                        // Check if the day has changed, every minute
                        if (ServerContext.ServerClock.ElapsedMilliseconds - _lastDayCheck > 60000)
                        {
                            _lastDayCheck = ServerContext.ServerClock.ElapsedMilliseconds;
                            if (ServerContext.Day != DateTime.Now.Day)
                            {
                                LunaLog.LogFilename = Path.Combine(LunaLog.LogFolder, $"lmpserver {DateTime.Now:yyyy-MM-dd HH-mm-ss}.log");
                                LunaLog.WriteToLog($"Continued from logfile {DateTime.Now:yyyy-MM-dd HH-mm-ss}.log");
                                ServerContext.Day = DateTime.Now.Day;
                            }
                        }

                        Thread.Sleep(500);
                    }

                    LmpPluginHandler.FireOnServerStop();
                    commandThread.Wait();
                    clientThread.Wait();
                    vesselRelayThread.Wait();

                    vesselRelayFarThread.Wait();
                    vesselRelayMediumThread.Wait();
                    vesselRelayCloseThread.Wait();
                }

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

        //Gracefully shut down
        private static void CatchExit(object sender, ConsoleCancelEventArgs args)
        {
            new ShutDownCommand().Execute("Caught Ctrl+C");
            Thread.Sleep(5000);
        }
    }
}