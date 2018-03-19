using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Time;
using Server.Client;
using Server.Command;
using Server.Context;
using Server.Exit;
using Server.Lidgren;
using Server.Log;
using Server.Plugin;
using Server.Settings;
using Server.System;
using Server.System.VesselRelay;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class MainServer
    {
        public static readonly TaskFactory LongRunTaskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        private static readonly WinExitSignal ExitSignal = Common.PlatformIsWindows() ? new WinExitSignal() : null;

        private static readonly List<Task> TaskContainer = new List<Task>();

        public static readonly CancellationTokenSource CancellationTokenSrc = new CancellationTokenSource();

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

                if (!Common.PlatformIsWindows()) LunaLog.Warning("Remember! Quit the server by using Control+C so the vessels are saved to the hard drive!");


                if (Common.PlatformIsWindows())
                    ExitSignal.Exit += (sender, args) => Exit();
                else
                {
                    //Register the ctrl+c event and exit signal if we are on linux
                    Console.CancelKeyPress += (sender, args) => Exit();
                }

                //We disable quick edit as otherwise when you select some text for copy/paste then you can't write to the console and server freezes
                //This just happens on windows....
                if (Common.PlatformIsWindows())
                    ConsoleUtil.DisableConsoleQuickEdit();

                //We cannot run more than 6 instances ofd servers + clients as otherwise the sync time will fail (30 seconds / 5 seconds = 6) but we use 3 for safety
                if (GetRunningInstances() > 3)
                    throw new HandledException("Cannot run more than 3 servers at a time!");

                //Start the server clock
                ServerContext.ServerClock.Start();

                //Set the last player activity time to server start
                ServerContext.LastPlayerActivity = ServerContext.ServerClock.ElapsedMilliseconds;

                ServerContext.ServerStarting = true;

                //Set day for log change
                ServerContext.Day = LunaTime.Now.Day;

                LunaLog.Normal($"Starting Luna Server version: {LmpVersioning.CurrentVersion}");

                Universe.CheckUniverse();
                LoadSettingsAndGroups();
                VesselStoreSystem.LoadExistingVessels();
                LmpPluginHandler.LoadPlugins();
                WarpSystem.Reset();

                LunaLog.Normal($"Starting {GeneralSettings.SettingsStore.WarpMode} server on Port {GeneralSettings.SettingsStore.Port}... ");

                ServerContext.ServerRunning = true;
                LidgrenServer.SetupLidgrenServer();

                //Do not add the command handler thread to the TaskContainer as it's a blocking task
                LongRunTaskFactory.StartNew(() => new CommandHandler().ThreadMain(), CancellationTokenSrc.Token);

                TaskContainer.Add(LongRunTaskFactory.StartNew(LogThread.RunLogThread, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(() => new ClientMainThread().ThreadMain(), CancellationTokenSrc.Token));

                TaskContainer.Add(LongRunTaskFactory.StartNew(() => BackupSystem.PerformBackups(CancellationTokenSrc.Token), CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LidgrenServer.StartReceiveingMessages, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LidgrenMasterServer.RefreshMasterServersList, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LidgrenMasterServer.RegisterWithMasterServer, CancellationTokenSrc.Token));

                TaskContainer.Add(LongRunTaskFactory.StartNew(VesselRelaySystem.RelayOldVesselMessages, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(VesselUpdateRelaySystem.RelayToFarPlayers, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(VesselUpdateRelaySystem.RelayToMediumDistancePlayers, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(VesselUpdateRelaySystem.RelayToClosePlayers, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(VersionChecker.RefreshLatestVersion, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(VersionChecker.DisplayNewVersionMsg, CancellationTokenSrc.Token));

                while (ServerContext.ServerStarting)
                    Thread.Sleep(500);

                LunaLog.Normal("All systems up and running. Поехали!");
                LmpPluginHandler.FireOnServerStart();

                QuitEvent.WaitOne();

                LmpPluginHandler.FireOnServerStop();

                LunaLog.Normal("So long and thanks for all the fish!");
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

            if (GeneralSettings.SettingsStore.ModControl)
            {
                LunaLog.Debug("Loading mod control...");
                ModFileSystem.LoadModFile();
            }
#if DEBUG
            DebugSettings.Singleton.Load();
            LunaTime.SimulatedMsTimeOffset = DebugSettings.SettingsStore.SimulatedMsTimeOffset;
#endif
        }

        /// <summary>
        /// Return the number of running instances.
        /// </summary>
        private static int GetRunningInstances() => Process.GetProcessesByName("LunaServer.exe").Length;

        /// <summary>
        /// Runs the exit logic
        /// </summary>
        private static void Exit()
        {
            LunaLog.Normal("Exiting... Please wait until all threads are finished");

            ServerContext.Shutdown("Server is shutting down");
            CancellationTokenSrc.Cancel();

            Task.WaitAll(TaskContainer.ToArray());

            QuitEvent.Set();
        }

        /// <summary>
        /// Runs the restart logic
        /// </summary>
        public static void Restart()
        {
            //Perform Backups
            BackupSystem.PerformBackups(CancellationTokenSrc.Token);
            LunaLog.Normal("Restarting...  Please wait until all threads are finished");

            ServerContext.Shutdown("Server is restarting");
            CancellationTokenSrc.Cancel();

            Task.WaitAll(TaskContainer.ToArray());
            QuitEvent.Set();

            //Start new server
            var serverExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Server.exe";
            var newProcLmpServer = new ProcessStartInfo {FileName = serverExePath};
            Process.Start(newProcLmpServer);
        }
    }
}