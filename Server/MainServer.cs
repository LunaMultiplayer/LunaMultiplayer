using LmpCommon;
using LmpCommon.Time;
using Server.Client;
using Server.Command;
using Server.Context;
using Server.Events;
using Server.Exit;
using Server.Log;
using Server.Plugin;
using Server.Server;
using Server.Settings;
using Server.Settings.Structures;
using Server.System;
using Server.Upnp;
using Server.Utilities;
using Server.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        private static bool IsRestart = false;

        public static void Main()
        {
            try
            {
                // Force culture to en-US to avoid 'System.Net.Sockets.resources' assembly load error.
                var ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                if (Common.PlatformIsWindows())
                    Console.Title = $"LMP {LmpVersioning.CurrentVersion}";

                Console.OutputEncoding = Encoding.UTF8;

                LunaLog.Info("Remember! Quit the server by using 'Control + C' so a backup is properly made before closing!");
                LunaLog.Info("Documentation available at https://github.com/LunaMultiplayer/LunaMultiplayer/wiki");

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

                ServerContext.ServerStarting = true;

                //Set day for log change
                ServerContext.Day = LunaNetworkTime.Now.Day;

                LunaLog.Normal($"Luna Server version: {LmpVersioning.CurrentVersion} ({AppContext.BaseDirectory})");

                Universe.CheckUniverse();
                LoadSettingsAndGroups();
                VesselStoreSystem.LoadExistingVessels();
                var scenariosCreated = ScenarioSystem.GenerateDefaultScenarios();
                ScenarioStoreSystem.LoadExistingScenarios(scenariosCreated);
                LmpPluginHandler.LoadPlugins();
                WarpSystem.Reset();
                TimeSystem.Reset();

                LunaLog.Normal($"Starting '{GeneralSettings.SettingsStore.ServerName}' on Address {ConnectionSettings.SettingsStore.ListenAddress} Port {ConnectionSettings.SettingsStore.Port}... ");

                LidgrenServer.SetupLidgrenServer();
                LmpPortMapper.OpenLmpPort().Wait();
                LmpPortMapper.OpenWebPort().Wait();
                ServerContext.ServerRunning = true;
                WebServer.StartWebServer();

                //Do not add the command handler thread to the TaskContainer as it's a blocking task
                LongRunTaskFactory.StartNew(CommandHandler.ThreadMain, CancellationTokenSrc.Token);

                TaskContainer.Add(LongRunTaskFactory.StartNew(WebServer.RefreshWebServerInformation, CancellationTokenSrc.Token));

                TaskContainer.Add(LongRunTaskFactory.StartNew(LmpPortMapper.RefreshUpnpPort, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LogThread.RunLogThread, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(ClientMainThread.ThreadMain, CancellationTokenSrc.Token));

                TaskContainer.Add(LongRunTaskFactory.StartNew(() => BackupSystem.PerformBackups(CancellationTokenSrc.Token), CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LidgrenServer.StartReceivingMessages, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LidgrenMasterServer.RegisterWithMasterServer, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(LidgrenMasterServer.CheckNATType, CancellationTokenSrc.Token));

                TaskContainer.Add(LongRunTaskFactory.StartNew(VersionChecker.RefreshLatestVersion, CancellationTokenSrc.Token));
                TaskContainer.Add(LongRunTaskFactory.StartNew(VersionChecker.DisplayNewVersionMsg, CancellationTokenSrc.Token));

                TaskContainer.Add(LongRunTaskFactory.StartNew(() => GcSystem.PerformGarbageCollection(CancellationTokenSrc.Token), CancellationTokenSrc.Token));

                while (ServerContext.ServerStarting)
                    Thread.Sleep(500);

                LunaLog.Normal("All systems up and running. Поехали!");
                LmpPluginHandler.FireOnServerStart();

                QuitEvent.WaitOne();

                LmpPluginHandler.FireOnServerStop();

                LunaLog.Normal("So long and thanks for all the fish!");

                if (IsRestart)
                {
                    //Start new server
                    var serverExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Server.exe";
                    var newProcLmpServer = new ProcessStartInfo { FileName = serverExePath };
                    Process.Start(newProcLmpServer);
                }
            }
            catch (Exception e)
            {
                LunaLog.Fatal(e is HandledException ? e.Message : $"Error in main server thread, Exception: {e}");
                Console.ReadLine(); //Avoid closing automatically
            }
        }

        private static void LoadSettingsAndGroups()
        {
            LunaLog.Debug("Loading groups...");
            GroupSystem.LoadGroups();
            LunaLog.Debug("Loading settings...");
            SettingsHandler.LoadSettings();
            SettingsHandler.ValidateDifficultySettings();

            if (GeneralSettings.SettingsStore.ModControl)
            {
                LunaLog.Debug("Loading mod control...");
                ModFileSystem.LoadModFile();
            }

            if (Common.PlatformIsWindows())
            {
                Console.Title += $" ({GeneralSettings.SettingsStore.ServerName})";
#if DEBUG
                Console.Title += " DEBUG";
#endif
            }
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
            ExitEvent.Exit();

            CancellationTokenSrc.Cancel();
            Task.WaitAll(TaskContainer.ToArray());

            ServerContext.Shutdown("Server is shutting down");

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

            IsRestart = true;

            QuitEvent.Set();
        }
    }
}
