using CommNet;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Events.Base;
using LmpClient.Localization;
using LmpClient.ModuleStore;
using LmpClient.ModuleStore.Patching;
using LmpClient.Network;
using LmpClient.Systems;
using LmpClient.Systems.Flag;
using LmpClient.Systems.KerbalSys;
using LmpClient.Systems.Mod;
using LmpClient.Systems.ModApi;
using LmpClient.Systems.Network;
using LmpClient.Systems.Scenario;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpClient.Windows;
using LmpCommon;
using LmpCommon.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace LmpClient
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class MainSystem : MonoBehaviour
    {
        public static MainSystem Singleton { get; set; }

        #region Fields
        
        public static string KspPath { get; private set; }

        private static ClientState _networkState;
        public static ClientState NetworkState
        {
            get => _networkState;
            set
            {
                if (value == ClientState.Disconnected)
                {
                    NetworkMain.ResetNetworkSystem();
                }
                _networkState = value;
                NetworkSystem.NetworkStatus = value;
            }
        }
        
        public string Status { get; set; }

        public const int WindowOffset = 1664147604;
        
        public static bool ToolbarShowGui { get; set; } = true;
        public static ServerEntry CommandLineServer { get; set; }
        public bool LmpSaveChecked { get; set; }
        public bool ForceQuit { get; set; }
        public bool StartGame { get; set; }
        public bool Enabled { get; set; } = true;

        private static int _mainThreadId;
        /// <summary>
        /// Checks if you are in the Unity thread or not
        /// </summary>
        public static bool IsUnityThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        //Hack gravity fix.
        public static Dictionary<CelestialBody, double> BodiesGees { get; } = new Dictionary<CelestialBody, double>();

        #endregion

        #region Update methods

        public void Update()
        {
            LunaLog.ProcessLogMessages();
            LunaScreenMsg.ProcessScreenMessages();

            if (!Enabled) return;

            try
            {
                if (HighLogic.LoadedScene == GameScenes.MAINMENU && !LmpSaveChecked)
                {
                    LmpSaveChecked = true;
                    SetupBlankGameIfNeeded();
                }

                HandleWindowEvents();
                SystemsHandler.Update();
                WindowsHandler.Update();

                //Force quit
                if (ForceQuit)
                {
                    ForceQuit = false;
                    NetworkState = ClientState.Disconnected;
                    StopGame();
                }

                //In case ANOTHER thread requested us to disconnect
                if (NetworkState == ClientState.DisconnectRequested)
                    NetworkState = ClientState.Disconnected;

                if (NetworkState >= ClientState.Running)
                {
                    if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                    {
                        NetworkConnection.Disconnect("Quit to main menu");
                    }

                    // save every GeeASL from each body in FlightGlobals
                    if (HighLogic.LoadedScene == GameScenes.FLIGHT && BodiesGees.Count == 0)
                        foreach (var body in FlightGlobals.Bodies)
                            BodiesGees.Add(body, body.GeeASL);

                    //handle use of cheats
                    if (!SettingsSystem.ServerSettings.AllowCheats)
                    {
                        CheatOptions.NonStrictAttachmentOrientation = false;
                        CheatOptions.BiomesVisible = false;
                        CheatOptions.AllowPartClipping = false;
                        CheatOptions.IgnoreMaxTemperature = false;
                        CheatOptions.UnbreakableJoints = false;
                        CheatOptions.InfiniteElectricity = false;
                        CheatOptions.InfinitePropellant = false;
                        CheatOptions.NoCrashDamage = false;

                        foreach (var gravityEntry in BodiesGees)
                            gravityEntry.Key.GeeASL = gravityEntry.Value;
                    }
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
        }

        #endregion

        #region Fixed update methods

        public void FixedUpdate()
        {
            if (!Enabled)
                return;

            SystemsHandler.FixedUpdate();
        }

        #endregion

        #region Late update methods

        public void LateUpdate()
        {
            if (!Enabled)
                return;

            SystemsHandler.LateUpdate();
        }

        #endregion

        #region Public methods

        public void OnApplicationQuit()
        {
            OnExit();
        }

        public void OnDestroy()
        {
            OnExit();
        }

        public void Start()
        {
            CompatibilityChecker.SpawnDialog();
            InstallChecker.SpawnDialog();

            LocalizationContainer.LoadLanguages();
            LocalizationContainer.LoadLanguage(SettingsSystem.CurrentSettings.Language);

            SystemsHandler.FillUpSystemsList();
            WindowsHandler.FillUpWindowsList();

            ModApiSystem.Singleton.Enabled = true;
            NetworkSystem.Singleton.Enabled = true;

            if (!SettingsSystem.CurrentSettings.DisclaimerAccepted)
            {
                Enabled = false;
                DisclaimerDialog.SpawnDialog();
            }
            else
            {
                StartCoroutine(UpdateHandler.CheckForUpdates());
            }
        }

        public void Awake()
        {
            Singleton = this;
            DontDestroyOnLoad(this);
            
            KspPath = UrlDir.ApplicationRootPath;

            //We are sure that we are in the unity thread as Awake() should only be called in a unity thread.
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            LunaLog.Log($"[LMP]: LMP {LmpVersioning.CurrentVersion} Starting at: {KspPath}");
            LunaLog.Log($"[LMP]: Debug port: {CommonUtil.DebugPort}");

            if (!CompatibilityChecker.IsCompatible() || !InstallChecker.IsCorrectlyInstalled())
            {
                Enabled = false;
                return;
            }

            FieldModuleStore.ReadCustomizationXml();
            LmpBaseEvent.Awake();
            HarmonyPatcher.Awake();
            PartModulePatcher.Awake();
            SetupDirectoriesIfNeeded();
            HandleCommandLineArgs();

            NetworkMain.AwakeNetworkSystem();

            ModSystem.Singleton.BuildDllFileList();

            LunaLog.Log("[LMP]: LMP Finished awakening");

            //Trigger a reset!
            NetworkState = ClientState.Disconnected;
        }

        // ReSharper disable once InconsistentNaming
        public void OnGUI()
        {
            //Window ID's - Doesn't include "random" offset.
            //Connection window: 6702
            //Status window: 6703
            //Chat window: 6704
            //Debug window: 6705
            //Mod windw: 6706
            //Craft folder window: 6707
            //Craft library window: 6708
            //Craft upload window: 6709
            //Screenshot window: 6710
            //Options window: 6711
            //Converter window: 6712
            //Disclaimer window: 6713
            //Servers window: 6714
            //Server details: 6715
            //Systems window: 6716
            //Locks window: 6717
            //Banned parts: 6718
            //Screenshot folder: 6719
            //Screenshot library: 6720
            //Screenshot image: 6721
            //Tools window: 6722
            //Admin window: 6723
            //Update window: 6724
            //Vessels window: 6725

            if (StyleLibrary.DefaultSkin == null)
                StyleLibrary.DefaultSkin = GUI.skin;

            WindowsHandler.OnGui();
        }

        public void OnExit()
        {
            NetworkConnection.Disconnect("Quit game");
            NetworkState = ClientState.Disconnected;
            LunaLog.ProcessLogMessages();
        }

        public Game.Modes ConvertGameMode(GameMode inputMode)
        {
            switch (inputMode)
            {
                case GameMode.Sandbox:
                    return Game.Modes.SANDBOX;
                case GameMode.Science:
                    return Game.Modes.SCIENCE_SANDBOX;
                case GameMode.Career:
                    return Game.Modes.CAREER;
            }
            return Game.Modes.SANDBOX;
        }

        public void HandleException(Exception e, string eventName)
        {
            LunaLog.LogError($"[LMP]: Threw in {eventName} event, exception: {e}");
            NetworkConnection.Disconnect($"Unhandled error in {eventName} event! exception: {eventName}");
            ForceQuit = true;
            NetworkState = ClientState.Disconnected;
        }

        public void DisconnectFromGame()
        {
            ForceQuit = true;
            NetworkConnection.Disconnect("Quit");
            ScenarioSystem.Singleton.SendScenarioModules();
        }

        #endregion

        #region Private methods

        private void StopGame()
        {
            HighLogic.SaveFolder = "LunaMultiplayer";

            if (HighLogic.LoadedScene != GameScenes.MAINMENU)
                HighLogic.LoadScene(GameScenes.MAINMENU);

            BodiesGees.Clear();
            FlightGlobals.Vessels.Clear();
            FlightGlobals.VesselsLoaded.Clear();
            FlightGlobals.VesselsUnloaded.Clear();
            FlightGlobals.fetch.activeVessel = null;
            var craftBrowser = FindObjectOfType<CraftBrowserDialog>();
            if (craftBrowser != null)
            {
                craftBrowser.Dismiss();
            }
        }

        private static void HandleWindowEvents()
        {
            if (CommandLineServer != null && HighLogic.LoadedScene == GameScenes.MAINMENU && Time.timeSinceLevelLoad > 1f)
            {
                NetworkConnection.ConnectToServer(CommandLineServer.Address, CommandLineServer.Port, CommandLineServer.Password);
                CommandLineServer = null;
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
            SetAdvancedParams(HighLogic.CurrentGame);
            SetCommNetParams(HighLogic.CurrentGame);

            //Set universe time
            HighLogic.CurrentGame.flightState.universalTime = WarpSystem.Singleton.CurrentSubspaceTime;

            //Load kerbals BEFORE loading the vessels or the loading of vessels will fail!
            KerbalSystem.Singleton.LoadKerbalsIntoGame();

            //Load the scenarios from the server
            ScenarioSystem.Singleton.LoadScenarioDataIntoGame();

            //Load the missing scenarios as well (Eg, Contracts and stuff for career mode
            ScenarioSystem.Singleton.LoadMissingScenarioDataIntoGame();

            LunaLog.Log($"[LMP]: Starting {SettingsSystem.ServerSettings.GameMode} game...");

            //.Start() seems to stupidly .Load() somewhere - Let's overwrite it so it loads correctly.
            GamePersistence.SaveGame(HighLogic.CurrentGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            HighLogic.CurrentGame.Start();
            LunaLog.Log("[LMP]: Started!");
        }

        public void SetAdvancedParams(Game currentGame)
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

        public void SetCommNetParams(Game currentGame)
        {
            currentGame.Parameters.CustomParams<CommNetParams>().plasmaBlackout =
                SettingsSystem.ServerSettings.ServerCommNetParameters.plasmaBlackout;
            currentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations =
                SettingsSystem.ServerSettings.ServerCommNetParameters.enableGroundStations;
            currentGame.Parameters.CustomParams<CommNetParams>().requireSignalForControl =
                SettingsSystem.ServerSettings.ServerCommNetParameters.requireSignalForControl;
            currentGame.Parameters.CustomParams<CommNetParams>().rangeModifier =
                SettingsSystem.ServerSettings.ServerCommNetParameters.rangeModifier;
            currentGame.Parameters.CustomParams<CommNetParams>().DSNModifier =
                SettingsSystem.ServerSettings.ServerCommNetParameters.DSNModifier;
            currentGame.Parameters.CustomParams<CommNetParams>().occlusionMultiplierVac =
                SettingsSystem.ServerSettings.ServerCommNetParameters.occlusionMultiplierVac;
            currentGame.Parameters.CustomParams<CommNetParams>().occlusionMultiplierAtm =
                SettingsSystem.ServerSettings.ServerCommNetParameters.occlusionMultiplierAtm;
        }

        private static void HandleCommandLineArgs()
        {
            var valid = false;
            var port = 8800;

            var commands = Environment.GetCommandLineArgs();

            if (commands.Any())
            {
                if (commands.IndexOf("-debug") >= 0)
                {
                    NetworkMain.Config.ConnectionTimeout = float.MaxValue;
                    NetworkMain.RandomizeBadConnectionValues();
                }

                var logFileIndex = commands.IndexOf("-logFile") + 1;
                if (logFileIndex > 0 && commands.Length > logFileIndex)
                {
                    CommonUtil.OutputLogFilePath = commands[logFileIndex];
                }

                var addressIndex = commands.IndexOf("-lmp") + 1;
                if (addressIndex > 0 && commands.Length > addressIndex)
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
                        LunaLog.Log($"[LMP]: Connecting via command line to: {address}, port: {port}");
                    }
                    else
                    {
                        LunaLog.LogError($"[LMP]: Command line address is invalid: {address}, port: {port}");
                    }
                }                
            }
        }

        private static void SetupDirectoriesIfNeeded()
        {
            var lunaMultiplayerSavesDirectory = CommonUtil.CombinePaths(KspPath, "saves", "LunaMultiplayer");
            CreateIfNeeded(lunaMultiplayerSavesDirectory);
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiplayerSavesDirectory, "Ships"));
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiplayerSavesDirectory, CommonUtil.CombinePaths("Ships", "VAB")));
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiplayerSavesDirectory, CommonUtil.CombinePaths("Ships", "SPH")));
            CreateIfNeeded(CommonUtil.CombinePaths(lunaMultiplayerSavesDirectory, "Subassemblies"));

            var lunaMultiplayerFlagsDirectory = CommonUtil.CombinePaths(KspPath, "GameData", "LunaMultiplayer", "Flags");
            CreateIfNeeded(lunaMultiplayerFlagsDirectory);
        }

        private static void CreateIfNeeded(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void SetupBlankGameIfNeeded()
        {
            var persistentFile = CommonUtil.CombinePaths(KspPath, "saves", "LunaMultiplayer", "persistent.sfs");
            if (!File.Exists(persistentFile))
            {
                LunaLog.Log("[LMP]: Creating new blank persistent.sfs file");
                var blankGame = CreateBlankGame();
                HighLogic.SaveFolder = "LunaMultiplayer";
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

            if (FlagSystem.Singleton.FlagExists(SettingsSystem.CurrentSettings.SelectedFlag))
            {
                returnGame.flagURL = SettingsSystem.CurrentSettings.SelectedFlag;
                FlagSystem.Singleton.SendFlag(SettingsSystem.CurrentSettings.SelectedFlag);
            }
            else
            {
                SettingsSystem.CurrentSettings.SelectedFlag = returnGame.flagURL = "Squad/Flags/default";
                SettingsSystem.SaveSettings();
            }

            returnGame.Title = "LunaMultiplayer";
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.Subspace)
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
            HighLogic.SaveFolder = "LunaMultiplayer";

            return returnGame;
        }

        #endregion
    }
}
