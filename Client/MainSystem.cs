using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LunaClient.Base;
using LunaClient.Systems;
using LunaClient.Systems.Flag;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Mod;
using LunaClient.Systems.Network;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaClient.Windows;
using LunaClient.Windows.Chat;
using LunaClient.Windows.Connection;
using LunaClient.Windows.Disclaimer;
using LunaClient.Windows.Status;
using LunaCommon;
using LunaCommon.Enums;
using UnityEngine;
using Profiler = LunaClient.Utilities.Profiler;

namespace LunaClient
{
    public class MainSystem : System<MainSystem>
    {
        #region Fields

        /// <summary>
        /// This property has a backing static field for faster access
        /// </summary>
        private static ClientState _networkState = ClientState.DISCONNECTED;
        public ClientState NetworkState { get { return _networkState; } set { _networkState = value; } }

        public string Status { get; set; }
        public const int WindowOffset = 1664147604;

        private string AssemblyPath { get; } =
            new DirectoryInfo(Assembly.GetExecutingAssembly().Location ?? "").FullName;

        private string KspPath { get; } = new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName;
        public bool ShowGui { get; set; } = true;
        public bool ToolbarShowGui { get; set; } = true;
        public static ServerEntry CommandLineServer { get; set; }
        public bool LmpSaveChecked { get; set; }
        public bool ForceQuit { get; set; }
        public bool FireReset { get; set; }
        public bool StartGame { get; set; }
        public bool GameRunning { get; set; }
        private float LastDisconnectMessageCheck { get; set; }
        public bool DisplayDisconnectMessage { get; set; }
        private ScreenMessage DisconnectMessage { get; set; }

        public override bool Enabled { get; set; } = true;



        //Hack gravity fix.
        private Dictionary<CelestialBody, double> BodiesGees { get; } = new Dictionary<CelestialBody, double>();

        #endregion

        #region Base overrides

        public override void Reset()
        {
            Profiler.LmpReferenceTime.Start();

            Debug.Log("KSP installed at " + KspPath);
            Debug.Log("LMP installed at " + AssemblyPath);

            if (!SettingsSystem.CurrentSettings.DisclaimerAccepted && HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                Enabled = false;
                DisclaimerWindow.Singleton.Display = true;
            }

            if (!CompatibilityChecker.IsCompatible() || !InstallChecker.IsCorrectlyInstalled())
            {
                Enabled = false;
                return;
            }

            SetupDirectoriesIfNeeded();
            UniverseSyncCache.Singleton.ExpireCache();

            //Register events needed to bootstrap the workers.
            GameEvents.onHideUI.Add(() => { ShowGui = false; });
            GameEvents.onShowUI.Add(() => { ShowGui = true; });

            SystemsHandler.Reset();
            WindowsHandler.Reset();

            HandleCommandLineArgs();
            LunaLog.Debug("LunaMultiPlayer " + VersionInfo.VersionNumber + " Initialized!");
        }

        public override void Update()
        {
            base.Update();
            var startClock = Profiler.LmpReferenceTime.ElapsedTicks;
            LunaLog.Update();

            if (!Enabled) return;

            try
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                {
                    if (!ModSystem.Singleton.DllListBuilt)
                    {
                        ModSystem.Singleton.DllListBuilt = true;
                        ModSystem.Singleton.BuildDllFileList();
                    }
                    if (!LmpSaveChecked)
                    {
                        LmpSaveChecked = true;
                        SetupBlankGameIfNeeded();
                    }
                }


                HandleWindowEvents();
                SystemsHandler.Update();
                WindowsHandler.Update();

                //Force quit
                if (ForceQuit)
                {
                    ForceQuit = false;
                    GameRunning = false;
                    FireReset = true;
                    StopGame();
                }

                if (DisplayDisconnectMessage)
                    ShowDisconnectMessage();

                //Normal quit
                if (GameRunning)
                {
                    if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                    {
                        GameRunning = false;
                        FireReset = true;
                        ToolbarSystem.Singleton.Enabled = false; //Always disable toolbar in main menu
                        NetworkSystem.Singleton.Disconnect("Quit to main menu", true);
                    }

                    if (HighLogic.CurrentGame.flagURL != SettingsSystem.CurrentSettings.SelectedFlag)
                    {
                        LunaLog.Debug("Saving Selected flag");
                        SettingsSystem.CurrentSettings.SelectedFlag = HighLogic.CurrentGame.flagURL;
                        SettingsSystem.Singleton.SaveSettings();
                        FlagSystem.Singleton.FlagChangeEvent = true;
                    }

                    // save every GeeASL from each body in FlightGlobals
                    if ((HighLogic.LoadedScene == GameScenes.FLIGHT) && (BodiesGees.Count == 0))
                        foreach (var body in FlightGlobals.Bodies)
                            BodiesGees.Add(body, body.GeeASL);

                    //handle use of cheats
                    if (!SettingsSystem.ServerSettings.AllowCheats)
                    {
                        CheatOptions.InfinitePropellant = false;
                        CheatOptions.NoCrashDamage = false;

                        foreach (var gravityEntry in BodiesGees)
                            gravityEntry.Key.GeeASL = gravityEntry.Value;
                    }

                    if ((HighLogic.LoadedScene == GameScenes.FLIGHT) && FlightGlobals.ready)
                        HighLogic.CurrentGame.Parameters.Flight.CanLeaveToSpaceCenter = PauseMenu.canSaveAndExit == ClearToSaveStatus.CLEAR;
                    else
                        HighLogic.CurrentGame.Parameters.Flight.CanLeaveToSpaceCenter = true;
                }

                if (FireReset)
                {
                    FireReset = false;
                    SystemsHandler.Reset();
                    WindowsHandler.Reset();
                }

                if (StartGame)
                {
                    StartGame = false;
                    StartGameNow();
                }
            }
            catch (Exception e)
            {
                HandleException(e, "Main system- update");
            }
            Profiler.UpdateData.ReportTime(startClock);
        }

        public void OnGui()
        {
            //Window ID's - Doesn't include "random" offset.
            //Connection window: 6702
            //Status window: 6703
            //Chat window: 6704
            //Debug window: 6705
            //Mod windw: 6706
            //Craft library window: 6707
            //Craft upload window: 6708
            //Screenshot window: 6710
            //Options window: 6711
            //Converter window: 6712
            //Disclaimer window: 6713

            var startClock = Profiler.LmpReferenceTime.ElapsedTicks;

            if (ShowGui && (ToolbarShowGui || HighLogic.LoadedScene == GameScenes.MAINMENU))
                WindowsHandler.OnGui();

            Profiler.GuiData.ReportTime(startClock);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            var startClock = Profiler.LmpReferenceTime.ElapsedTicks;

            if (!Enabled)
                return;

            SystemsHandler.FixedUpdate();
            Profiler.FixedUpdateData.ReportTime(startClock);
        }

        #endregion

        #region Public methods

        public static void Delay(int delayMs)
        {
            var t = Environment.TickCount;
            while (Environment.TickCount - t < delayMs)
            {
            }
        }

        public void OnExit()
        {
            NetworkSystem.Singleton.Disconnect("Quit game", true);
            SystemsHandler.Reset();
        }

        public Game.Modes ConvertGameMode(GameMode inputMode)
        {
            switch (inputMode)
            {
                case GameMode.SANDBOX:
                    return Game.Modes.SANDBOX;
                case GameMode.SCIENCE:
                    return Game.Modes.SCIENCE_SANDBOX;
                case GameMode.CAREER:
                    return Game.Modes.CAREER;
            }
            return Game.Modes.SANDBOX;
        }

        public void HandleException(Exception e, string eventName)
        {
            LunaLog.Debug($"Threw in {eventName} event, exception: " + e);
            NetworkSystem.Singleton.Disconnect($"Unhandled error in main system! Detail: {eventName}", true);
            Reset();
        }

        #endregion

        #region Private methods

        private void ShowDisconnectMessage()
        {
            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
            {
                if (Time.realtimeSinceStartup - LastDisconnectMessageCheck > 1f)
                {
                    LastDisconnectMessageCheck = Time.realtimeSinceStartup;
                    if (DisconnectMessage != null)
                        DisconnectMessage.duration = 0;
                    DisconnectMessage = ScreenMessages.PostScreenMessage("You have been disconnected!", 2f,
                        ScreenMessageStyle.UPPER_CENTER);
                }
            }
            else
            {
                DisplayDisconnectMessage = false;
            }
        }

        private void StopGame()
        {
            HighLogic.SaveFolder = "LunaMultiPlayer";
            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
                HighLogic.LoadScene(GameScenes.MAINMENU);
            //HighLogic.CurrentGame = null; This is no bueno
            BodiesGees.Clear();
        }

        private void HandleWindowEvents()
        {
            if (!StatusWindow.Singleton.DisconnectEventHandled)
            {
                StatusWindow.Singleton.DisconnectEventHandled = true;
                ForceQuit = true;
                NetworkSystem.Singleton.Disconnect("Quit", true);
                ScenarioSystem.Singleton.SendScenarioModules(); // Send scenario modules before disconnecting
            }
            if (!ConnectionWindow.RenameEventHandled)
            {
                StatusSystem.Singleton.MyPlayerStatus.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
                ConnectionWindow.RenameEventHandled = true;
                SettingsSystem.Singleton.SaveSettings();
            }
            if (!ConnectionWindow.AddEventHandled)
            {
                SettingsSystem.CurrentSettings.Servers.Add(ConnectionWindow.AddEntry);
                ConnectionWindow.AddEntry = null;
                ConnectionWindow.AddingServer = false;
                ConnectionWindow.AddEventHandled = true;
                SettingsSystem.Singleton.SaveSettings();
            }
            if (!ConnectionWindow.EditEventHandled)
            {
                SettingsSystem.CurrentSettings.Servers[ConnectionWindow.Selected].Name = ConnectionWindow.EditEntry.Name;
                SettingsSystem.CurrentSettings.Servers[ConnectionWindow.Selected].Address = ConnectionWindow.EditEntry.Address;
                SettingsSystem.CurrentSettings.Servers[ConnectionWindow.Selected].Port = ConnectionWindow.EditEntry.Port;
                ConnectionWindow.EditEntry = null;
                ConnectionWindow.AddingServer = false;
                ConnectionWindow.EditEventHandled = true;
                SettingsSystem.Singleton.SaveSettings();
            }
            if (!ConnectionWindow.RemoveEventHandled)
            {
                SettingsSystem.CurrentSettings.Servers.RemoveAt(ConnectionWindow.Selected);
                ConnectionWindow.Selected = -1;
                ConnectionWindow.RemoveEventHandled = true;
                SettingsSystem.Singleton.SaveSettings();
            }
            if (!ConnectionWindow.ConnectEventHandled)
            {
                ConnectionWindow.ConnectEventHandled = true;
                NetworkSystem.Singleton.ConnectToServer(
                    SettingsSystem.CurrentSettings.Servers[ConnectionWindow.Selected].Address,
                    SettingsSystem.CurrentSettings.Servers[ConnectionWindow.Selected].Port);
            }
            if ((CommandLineServer != null) && (HighLogic.LoadedScene == GameScenes.MAINMENU) &&
                (Time.timeSinceLevelLoad > 1f))
            {
                NetworkSystem.Singleton.ConnectToServer(CommandLineServer.Address, CommandLineServer.Port);
                CommandLineServer = null;
            }

            if (!ConnectionWindow.DisconnectEventHandled)
            {
                ConnectionWindow.DisconnectEventHandled = true;
                GameRunning = false;
                FireReset = true;
                NetworkSystem.Singleton.Disconnect(Singleton.NetworkState <= ClientState.STARTING
                    ? "Cancelled connection to server"
                    : "Quit", true);
            }
        }

        private void StartGameNow()
        {
            //Create new game object for our LMP session.
            HighLogic.CurrentGame = CreateBlankGame();

            //Set the game mode
            HighLogic.CurrentGame.Mode = ConvertGameMode(SettingsSystem.ServerSettings.GameMode);

            //Set difficulty
            HighLogic.CurrentGame.Parameters = SettingsSystem.ServerSettings.ServerParameters;
            SetAdvancedAndCommNetParams(HighLogic.CurrentGame);

            //Set universe time
            HighLogic.CurrentGame.flightState.universalTime = WarpSystem.Singleton.GetCurrentSubspaceTime();

            //Load LMP stuff
            KerbalSystem.Singleton.LoadKerbalsIntoGame();
            VesselProtoSystem.Singleton.VesselLoader.LoadVesselsIntoGame();

            //Load the scenarios from the server
            ScenarioSystem.Singleton.LoadScenarioDataIntoGame();

            //Load the missing scenarios as well (Eg, Contracts and stuff for career mode
            ScenarioSystem.Singleton.LoadMissingScenarioDataIntoGame();

            //This only makes KSP complain
            HighLogic.CurrentGame.CrewRoster.ValidateAssignments(HighLogic.CurrentGame);
            LunaLog.Debug("Starting " + SettingsSystem.ServerSettings.GameMode + " game...");

            //.Start() seems to stupidly .Load() somewhere - Let's overwrite it so it loads correctly.
            GamePersistence.SaveGame(HighLogic.CurrentGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            HighLogic.CurrentGame.Start();
            ChatWindow.Singleton.Display = true;
            LunaLog.Debug("Started!");
        }


        public void SetAdvancedAndCommNetParams(Game currentGame)
        {
            if (SettingsSystem.ServerSettings.ServerCommNetParameters != null)
            {
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().EnableKerbalExperience =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.EnableKerbalExperience;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().ImmediateLevelUp =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.ImmediateLevelUp;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().AllowNegativeCurrency =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.AllowNegativeCurrency;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().ActionGroupsAlways =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.ActionGroupsAlways;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().GKerbalLimits =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.GKerbalLimits;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().GPartLimits =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.GPartLimits;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalGToleranceMult =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.KerbalGToleranceMult;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().PressurePartLimits =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.PressurePartLimits;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().BuildingImpactDamageMult =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.BuildingImpactDamageMult;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().PartUpgradesInCareer =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.PartUpgradesInCareer;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().PartUpgradesInSandbox =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.PartUpgradesInSandbox;
                currentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().ResourceTransferObeyCrossfeed =
                    SettingsSystem.ServerSettings.ServerAdvancedParameters.ResourceTransferObeyCrossfeed;
            }
            if (SettingsSystem.ServerSettings.ServerCommNetParameters != null)
            {
                currentGame.Parameters.CustomParams<CommNet.CommNetParams>().enableGroundStations =
                    SettingsSystem.ServerSettings.ServerCommNetParameters.enableGroundStations;
                currentGame.Parameters.CustomParams<CommNet.CommNetParams>().requireSignalForControl =
                    SettingsSystem.ServerSettings.ServerCommNetParameters.requireSignalForControl;
                currentGame.Parameters.CustomParams<CommNet.CommNetParams>().rangeModifier =
                    SettingsSystem.ServerSettings.ServerCommNetParameters.rangeModifier;
                currentGame.Parameters.CustomParams<CommNet.CommNetParams>().DSNModifier =
                    SettingsSystem.ServerSettings.ServerCommNetParameters.DSNModifier;
                currentGame.Parameters.CustomParams<CommNet.CommNetParams>().occlusionMultiplierVac =
                    SettingsSystem.ServerSettings.ServerCommNetParameters.occlusionMultiplierVac;
                currentGame.Parameters.CustomParams<CommNet.CommNetParams>().occlusionMultiplierAtm =
                    SettingsSystem.ServerSettings.ServerCommNetParameters.occlusionMultiplierAtm;
            }
        }

        private static void HandleCommandLineArgs()
        {
            var valid = false;
            var port = 6702;

            var commands = Environment.GetCommandLineArgs();
            var addressIndex = commands.IndexOf("-lmp") + 1;
            if (commands.Any() && (addressIndex > 0) && (commands.Length > addressIndex))
            {
                var address = commands[addressIndex];
                if (address.Contains("lmp://"))
                {
                    if (address.Substring("lmp://".Length).Contains(":")) //With port
                    {
                        address = address.Substring("lmp://".Length)
                            .Substring(0, address.LastIndexOf(":", StringComparison.Ordinal));
                        var portString = address.Substring(address.LastIndexOf(":", StringComparison.Ordinal) + 1);
                        valid = int.TryParse(portString, out port);
                    }
                    else
                    {
                        address = address.Substring("lmp://".Length);
                        valid = true;
                    }
                }
                if (valid)
                {
                    CommandLineServer = new ServerEntry { Address = address, Port = port };
                    LunaLog.Debug("Connecting via command line to: " + address + ", port: " + port);
                }
                else
                {
                    LunaLog.Debug("Command line address is invalid: " + address + ", port: " + port);
                }
            }
        }

        private static void SetupDirectoriesIfNeeded()
        {
            var lunaMultiPlayerSavesDirectory = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "saves", "LunaMultiPlayer");
            CreateIfNeeded(lunaMultiPlayerSavesDirectory);
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiPlayerSavesDirectory, "Ships"));
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiPlayerSavesDirectory, CommonUtil.CombinePaths("Ships", "VAB")));
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiPlayerSavesDirectory, CommonUtil.CombinePaths("Ships", "SPH")));
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiPlayerSavesDirectory, "Subassemblies"));
            var lunaMultiPlayerCacheDirectory = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData", "LunaMultiPlayer", "Cache");
            CreateIfNeeded(lunaMultiPlayerCacheDirectory);
            var lunaMultiPlayerIncomingCacheDirectory = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData",
                "LunaMultiPlayer", "Cache","Incoming");

            CreateIfNeeded(lunaMultiPlayerIncomingCacheDirectory);
            var lunaMultiPlayerFlagsDirectory = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData", "LunaMultiPlayer", "Flags");
            CreateIfNeeded(lunaMultiPlayerFlagsDirectory);
        }

        private static void CreateIfNeeded(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void SetupBlankGameIfNeeded()
        {
            var persistentFile = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "saves", "LunaMultiPlayer", "persistent.sfs");
            if (!File.Exists(persistentFile))
            {
                LunaLog.Debug("Creating new blank persistent.sfs file");
                var blankGame = CreateBlankGame();
                HighLogic.SaveFolder = "LunaMultiPlayer";
                GamePersistence.SaveGame(blankGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            }
        }

        private static Game CreateBlankGame()
        {
            var returnGame = new Game { additionalSystems = new ConfigNode() };
            //KSP complains about a missing Message system if we don't do this.
            returnGame.additionalSystems.AddNode("MESSAGESYSTEM");

            //Flightstate is null on new Game();
            returnGame.flightState = new FlightState();
            if (returnGame.flightState.mapViewFilterState == 0)
                returnGame.flightState.mapViewFilterState = -1026;

            returnGame.startScene = GameScenes.SPACECENTER;
            returnGame.flagURL = SettingsSystem.CurrentSettings.SelectedFlag;
            returnGame.Title = "LunaMultiPlayer";
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.SUBSPACE)
            {
                returnGame.Parameters.Flight.CanQuickLoad = true;
                returnGame.Parameters.Flight.CanRestart = true;
                returnGame.Parameters.Flight.CanLeaveToEditor = true;
            }
            else
            {
                returnGame.Parameters.Flight.CanQuickLoad = false;
                returnGame.Parameters.Flight.CanRestart = false;
                returnGame.Parameters.Flight.CanLeaveToEditor = false;
            }
            HighLogic.SaveFolder = "LunaMultiPlayer";

            return returnGame;
        }

        #endregion

        #region Unused methods

        //if (ScreenshotWorker.fetch.uploadScreenshot)
        //{
        //    ScreenshotWorker.fetch.uploadScreenshot = false;
        //    StartCoroutine(UploadScreenshot());
        //}

        //public IEnumerator<WaitForEndOfFrame> UploadScreenshot()
        //{
        //    yield return new WaitForEndOfFrame();
        //    ScreenshotWorker.fetch.SendScreenshot();
        //    ScreenshotWorker.fetch.screenshotTaken = true;
        //}

        #endregion
    }
}