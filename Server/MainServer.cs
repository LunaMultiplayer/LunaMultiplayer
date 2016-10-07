using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LunaCommon;
using LunaCommon.Enums;
using LunaServer.Client;
using LunaServer.Command;
using LunaServer.Command.Command;
using LunaServer.Context;
using LunaServer.Lidgren;
using LunaServer.Log;
using LunaServer.Plugin;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;

namespace LunaServer
{
    public class MainServer
    {
        private static long _ctrlCTime;
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
                if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.CUSTOM)
                {
                    GameplaySettings.Reset();
                    GameplaySettings.Load();
                }

                //Set day for log change
                ServerContext.Day = DateTime.Now.Day;

                //Load plugins
                LmpPluginHandler.LoadPlugins();

                Console.Title = "LMPServer v" + VersionInfo.VersionNumber;

                while (ServerContext.ServerStarting || ServerContext.ServerRestarting)
                {
                    if (ServerContext.ServerRestarting)
                    {
                        LunaLog.Debug("Reloading settings...");
                        GeneralSettings.Reset();
                        GeneralSettings.Load();
                        if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.CUSTOM)
                        {
                            LunaLog.Debug("Reloading gameplay settings...");
                            GameplaySettings.Reset();
                            GameplaySettings.Load();
                        }
                    }

                    ServerContext.ServerRestarting = false;
                    LunaLog.Normal("Starting Luna Server v" + VersionInfo.VersionNumber);

                    if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.CUSTOM)
                    {
                        //Generate the config file by accessing the object.
                        LunaLog.Debug("Loading gameplay settings...");
                        GameplaySettings.Load();
                    }

                    //Load universe
                    LunaLog.Normal("Loading universe... ");
                    Universe.CheckUniverse();

                    LunaLog.Normal("Starting " + GeneralSettings.SettingsStore.WarpMode + " server on Port " +
                                   GeneralSettings.SettingsStore.Port + "... ");

                    ServerContext.ServerRunning = true;
                    
                    var commandThread = Task.Run(() => new CommandHandler().ThreadMain());
                    var clientThread = Task.Run(() => new ClientMainThread().ThreadMain());

                    ServerContext.LidgrenServer.SetupLidgrenServer();
                    Task.Run(() => ServerContext.LidgrenServer.StartReceiveingMessages());

                    var vesselRelayFarThread = Task.Run(() => VesselUpdateRelay.RelayToFarPlayers());
                    var vesselRelayMediumThread = Task.Run(() => VesselUpdateRelay.RelayToMediumDistancePlayers());
                    var vesselRelayCloseThread = Task.Run(() => VesselUpdateRelay.RelayToClosePlayers());

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
                    vesselRelayFarThread.Wait();
                    vesselRelayMediumThread.Wait();
                    vesselRelayCloseThread.Wait();
                }
                LunaLog.Normal("Goodbye and thanks for all the fish!");
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                LunaLog.Fatal("Error in main server thread, Exception: " + e);
                throw;
            }
        }

        //Gracefully shut down
        private static void CatchExit(object sender, ConsoleCancelEventArgs args)
        {
            //If control+c not pressed within 5 seconds, catch it and shutdown gracefully.
            if (DateTime.UtcNow.Ticks - _ctrlCTime > 50000000)
            {
                _ctrlCTime = DateTime.UtcNow.Ticks;
                args.Cancel = true;
                new ShutDownCommand().Execute("Caught Ctrl+C");
            }
            else
            {
                LunaLog.Debug("Terminating!");
            }
        }
    }
}