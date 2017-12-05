using System;
using LunaCommon.Enums;
using LunaCommon.Xml;
using Server.Enums;
using Server.Log;

namespace Server.Settings.Definition
{
    [Serializable]
    public class SettingsDefinition
    {
        [XmlComment(Value = "The UDP port the server listens on. You don't need to open it in your router if RegisterWithMasterServer = true")]
        public int Port { get; set; } = 8800;

        [XmlComment(Value = "Name of the server. Max 30 char")]
        public string ServerName { get; set; } = "Luna Server";

        [XmlComment(Value = "Description of the server. Max 200 char")]
        public string Description { get; set; } = "Luna Server Description";

        [XmlComment(Value = "Specify the server's MOTD (message of the day). 255 chars max")]
        public string ServerMotd { get; set; } = "Welcome, %Name%!";

        [XmlComment(Value = "Maximum amount of Players that can join the server.")]
        public int MaxPlayers { get; set; } = 20;

        [XmlComment(Value = "Set to false if you don't want to appear on the server list")]
        public bool RegisterWithMasterServer { get; set; } = true;

        [XmlComment(Value = "Specify in miliseconds how often we will update the info with masterserver. Min value = 5000")]
        public int MasterServerRegistrationMsInterval { get; set; } = 5000;

        [XmlComment(Value = "Specify in minutes how often /dekessler automatically runs. 0 = Disabled")]
        public float AutoDekessler { get; set; } = 0.5f;

        [XmlComment(Value = "Specify in minutes how often /nukeksc automatically runs. 0 = Disabled")]
        public float AutoNuke { get; set; } = 0.0f;

        [XmlComment(Value = "Specify if the vessels that are being CONTROLLED and in a past subspace will be shown for players in future subspaces")]
        public bool ShowVesselsInThePast { get; set; } = false;

        [XmlComment(Value = "Enable use of Cheats in-game.")]
        public bool Cheats { get; set; } = true;

        [XmlComment(Value = "If this is true when a user sacks a kerbal the other players will have that kerbal removed in their games aswell")]
        public bool RelayKerbalRemove { get; set; } = false;

        [XmlComment(Value = "Specify the Name that will appear when you send a message using the server's console.")]
        public string ConsoleIdentifier { get; set; } = "Server";

        [XmlComment(Value = "Specify the gameplay difficulty of the server. Values: Easy, Normal, Moderate, Hard, Custom")]
        public GameDifficulty GameDifficulty { get; set; } = GameDifficulty.Normal;

        [XmlComment(Value = "Specify the game Type. Values: Sandbox, Career, Science")]
        public GameMode GameMode { get; set; } = GameMode.Sandbox;

        [XmlComment(Value = "Enable mod control. WARNING: Only consider turning off mod control for private servers. " +
                            "The game will constantly complain about missing parts if there are missing mods." +
                            "Values: Disabled, EnabledStopInvalidPartSync, EnabledStopInvalidPartLaunch")]
        public ModControlMode ModControl { get; set; } = ModControlMode.EnabledStopInvalidPartSync;

        [XmlComment(Value = "How many untracked asteroids to spawn into the universe. 0 = Disabled")]
        public int NumberOfAsteroids { get; set; } = 10;

        [XmlComment(Value = "Terrain quality. All clients will need to have this setting in their KSP to avoid terrain differences" +
                            "Values: Low, Default, High")]
        public TerrainQuality TerrainQuality { get; set; } = TerrainQuality.High;

#if DEBUG
        [XmlComment(Value = "Specify the minimum distance in which vessels can interact with eachother at the launch pad and runway")]
        public float SafetyBubbleDistance { get; set; } = 10.0f;
#else
        [XmlComment(Value = "Specify the minimum distance in which vessels can interact with eachother at the launch pad and runway")]
        public float SafetyBubbleDistance { get; set; }= 100.0f;
#endif
        
        [XmlComment(Value = "Specify the warp Type. Values: None, Subspace, Master")]
        public WarpMode WarpMode { get; set; } = WarpMode.Subspace;

        [XmlComment(Value = "Username of the player who control the warp if WarpMode is set to MASTER")]
        public string WarpMaster { get; set; } = "";

        [XmlComment(Value = "Enable white-listing.")]
        public bool Whitelisted { get; set; } = false;

        [XmlComment(Value = "Compress messages or not")]
        public bool CompressionEnabled { get; set; } = true;

        [XmlComment(Value = "Heartbeat interval in Ms")]
        public int HearbeatMsInterval { get; set; } = 1000;

#if DEBUG
        [XmlComment(Value = "Connection timeout in Ms")]
        public int ConnectionMsTimeout { get; set; } = int.MaxValue;
#else
        [XmlComment(Value = "Connection timeout in Ms")]
        public int ConnectionMsTimeout { get; set; } = 5000;
#endif

        [XmlComment(Value = "If this is set to true, vessels can be taken by anyone after a player switch to another vessel.")]
        public bool DropControlOnVesselSwitching { get; set; } = true;

        [XmlComment(Value = "If this is set to true, vessels can be taken by anyone after a player switch to track station, space center or VAB/SPH.")]
        public bool DropControlOnExitFlight { get; set; } = true;

        [XmlComment(Value = "If this is set to true, vessels can be taken by anyone after a player disconnects.")]
        public bool DropControlOnExit { get; set; } = true;

        [XmlComment(Value = "Interval in Ms at wich the client will send updates for his vessel when other players are nearby. " +
                     "Decrease it if your clients have good network connection and you plan to do dogfights")]
        public int VesselUpdatesSendMsInterval { get; set; } = 30;

        [XmlComment(Value = "Interval in Ms at wich the client will send updates for vessels that are uncontrolled but nearby him.")]
        public int SecondaryVesselUpdatesSendMsInterval { get; set; } = 500;

        [XmlComment(Value = "Interval in ms at wich users will send the controlled and close uncontrolled vessel definitions (ProtoVessel) to the server." +
                            "Caution! Sending a vessel definition (ProtoVessel) very often could make clients with slow computers to lag a lot!")]
        public int VesselDefinitionSendMsInterval { get; set; } = 500;

        [XmlComment(Value = "Relay system mode. Dictionary uses more RAM but it's faster. DataBase use disk space instead but it's slower" +
                            "Values: Dictionary, DataBase")]
        public RelaySystemMode RelaySystemMode { get; set; } = RelaySystemMode.Dictionary;

        [XmlComment(Value = "How often the server will relay position updates between players that are in different planets")]
        public int FarDistanceUpdateIntervalMs { get; set; } = 500; // 2FPS

        [XmlComment(Value = "How often the server will relay position updates between players that are in the same planet but with a distance >25km")]
        public int MediumDistanceUpdateIntervalMs { get; set; } = 250; //4 FPS

        [XmlComment(Value = "How often the server will relay position updates between players that are <25km from each other")]
        public int CloseDistanceUpdateIntervalMs { get; set; } = 33; //30 FPS

        [XmlComment(Value = "Distance in meters to differentiate between CloseDistanceUpdateIntervalMs and MediumDistanceUpdateIntervalMs")]
        public int CloseDistanceInMeters { get; set; } = 25000;

        [XmlComment(Value = "Send/Receive tick clock")]
        public int SendReceiveThreadTickMs { get; set; } = 5;

        [XmlComment(Value = "Main thread polling in ms")]
        public int MainTimeTick { get; set; } = 5;
        
        [XmlComment(Value = "Minimum log level. Values: Debug, Info, Chat, Error, Fatal")]
        public LogLevels LogLevel { get; set; } = LogLevels.Debug;

        [XmlComment(Value = "Specify the amount of days a log file should be considered as expired and deleted. 0 = Disabled")]
        public double ExpireLogs { get; set; } = 0;

        [XmlComment(Value = "Use UTC instead of system time in the log.")]
        public bool UseUtcTimeInLog { get; set; } = false;
    }
}