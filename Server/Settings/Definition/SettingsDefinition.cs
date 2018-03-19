using LunaCommon.Enums;
using LunaCommon.Xml;
using Server.Enums;
using Server.Log;
using System;

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

        [XmlComment(Value = "Password for the server. Leave it empty if you want to make a public server")]
        public string Password { get; set; } = "";

        [XmlComment(Value = "Specify the server's MOTD (message of the day). 255 chars max")]
        public string ServerMotd { get; set; } = "Welcome, %Name%!";

        [XmlComment(Value = "Maximum amount of players that can join the server.")]
        public int MaxPlayers { get; set; } = 20;

        [XmlComment(Value = "Maximum length of a username.")]
        public int MaxUsernameLength { get; set; } = 15;

        [XmlComment(Value = "Set to false if you don't want to appear on the server list")]
        public bool RegisterWithMasterServer { get; set; } = true;

        [XmlComment(Value = "Specify in miliseconds how often we will update the info with masterserver. Min value = 5000")]
        public int MasterServerRegistrationMsInterval { get; set; } = 5000;

        [XmlComment(Value = "Specify in minutes how often /dekessler automatically runs. 0 = Disabled")]
        public float AutoDekessler { get; set; } = 0.5f;

        [XmlComment(Value = "Specify in minutes how often /nukeksc automatically runs. 0 = Disabled")]
        public float AutoNuke { get; set; } = 0.0f;

        [XmlComment(Value = "Specify if the vessels that are being CONTROLLED and in a past subspace will be shown for players in future subspaces")]
        public bool ShowVesselsInThePast { get; set; } = true;

        [XmlComment(Value = "Enable use of Cheats in-game.")]
        public bool Cheats { get; set; } = true;

        [XmlComment(Value = "Allow players to sack kerbals")]
        public bool AllowSackKerbals { get; set; } = false;

        [XmlComment(Value = "Specify the Name that will appear when you send a message using the server's console.")]
        public string ConsoleIdentifier { get; set; } = "Server";

        [XmlComment(Value = "Specify the gameplay difficulty of the server. Values: Easy, Normal, Moderate, Hard, Custom")]
        public GameDifficulty GameDifficulty { get; set; } = GameDifficulty.Normal;

        [XmlComment(Value = "Specify the game Type. Values: Sandbox, Career, Science")]
        public GameMode GameMode { get; set; } = GameMode.Sandbox;

        [XmlComment(Value = "Enable mod control. WARNING: Only consider turning off mod control for private servers. " +
                            "The game will constantly complain about missing parts if there are missing mods. " +
                            "Read this wiki page: https://github.com/LunaMultiplayer/LunaMultiplayer/wiki/Mod-file to understand how it works")]
        public bool ModControl { get; set; } = true;

        [XmlComment(Value = "How many untracked asteroids to spawn into the universe. 0 = Disabled")]
        public int NumberOfAsteroids { get; set; } = 10;

        [XmlComment(Value = "Terrain quality. All clients will need to have this setting in their KSP to avoid terrain differences" +
                            "Values: Low, Default, High")]
        public TerrainQuality TerrainQuality { get; set; } = TerrainQuality.High;

        [XmlComment(Value = "Specify the minimum distance in which vessels can interact with eachother at the launch pad and runway")]
        public float SafetyBubbleDistance { get; set; } = 100.0f;
        
        [XmlComment(Value = "Specify the warp Type. Values: None, Subspace, Master")]
        public WarpMode WarpMode { get; set; } = WarpMode.Subspace;

        [XmlComment(Value = "Username of the player who control the warp if WarpMode is set to MASTER")]
        public string WarpMaster { get; set; } = "";

        [XmlComment(Value = "Minimum interval between screenshots in milliseconds")]
        public int MinScreenshotIntervalMs { get; set; } = 30000;

        [XmlComment(Value = "Maximum screenshots kept per user")]
        public int MaxScreenshotsPerUser { get; set; } = 30;

        [XmlComment(Value = "Maximum screenshots folders kept")]
        public int MaxScreenshotsFolders { get; set; } = 50;
        
        [XmlComment(Value = "Minimum interval between uploading crafts in milliseconds")]
        public int MinCraftsIntervalMs { get; set; } = 5000;

        [XmlComment(Value = "Maximum crafts kept per user per type (VAB,SPH and Subassembly)")]
        public int MaxCraftsPerUser { get; set; } = 10;

        [XmlComment(Value = "Maximum crafts folders kept")]
        public int MaxCraftFolders { get; set; } = 50;

        [XmlComment(Value = "Enable white-listing of users")]
        public bool Whitelisted { get; set; } = false;

        [XmlComment(Value = "Heartbeat interval in Ms")]
        public int HearbeatMsInterval { get; set; } = 1000;

        [XmlComment(Value = "Connection timeout in Ms")]
        public int ConnectionMsTimeout { get; set; } = 30000;

        [XmlComment(Value = "Interval in Ms at wich the client will send updates for his vessel when other players are nearby. " +
                     "Decrease it if your clients have good network connection and you plan to do dogfights")]
        public int VesselUpdatesSendMsInterval { get; set; } = 30;

        [XmlComment(Value = "Interval in Ms at wich the client will send updates for vessels that are uncontrolled but nearby him.")]
        public int SecondaryVesselUpdatesSendMsInterval { get; set; } = 500;

        [XmlComment(Value = "Interval in ms at wich users will check the controlled and close uncontrolled vessel and sync the parts that have changes " +
                            "(ladders that extend or shields that open) to the server. " +
                            "Caution! Puting a very low value could make clients with slow computers to lag a lot!")]
        public int VesselPartsSyncMsInterval { get; set; } = 500;

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

        [XmlComment(Value = "Interval in ms at wich vessels will be written to the Universe/Vessels folder")]
        public int VesselsBackupIntervalMs { get; set; } = 30000;
    }
}
