using LunaCommon.Enums;
using LunaCommon.Xml;
using Server.Enums;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class SettingsDefinition
    {
        [XmlComment(Value = "The UDP port the server listens on. You don't need to open it on your router if RegisterWithMasterServer = true. " +
                            "If you want that players can connect against your server MANUALLY you will need to open it on your router")]
        public int Port { get; set; } = 8800;

        [XmlComment(Value = "Name of the server. Max 30 char")]
        public string ServerName { get; set; } = "Luna Server";

        [XmlComment(Value = "Description of the server. Max 200 char")]
        public string Description { get; set; } = "Luna Server Description";

        [XmlComment(Value = "Password for the server. Leave it empty if you want to make a public server. Max 30 chars")]
        public string Password { get; set; } = "";

        [XmlComment(Value = "Admin password for the server. Leave it empty if you don't want to allow server administration from KSP. Max 30 chars")]
        public string AdminPassword { get; set; } = "";

        [XmlComment(Value = "Specify the server's MOTD (message of the day). 255 chars max")]
        public string ServerMotd { get; set; } = "Welcome, %Name%!";

        [XmlComment(Value = "Maximum amount of players that can join the server.")]
        public int MaxPlayers { get; set; } = 20;

        [XmlComment(Value = "Maximum length of a username.")]
        public int MaxUsernameLength { get; set; } = 15;

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

        [XmlComment(Value = "Terrain quality. All clients will need to have this setting in their KSP to avoid terrain differences. Values: Low, Default, High")]
        public TerrainQuality TerrainQuality { get; set; } = TerrainQuality.High;

        [XmlComment(Value = "Specify the minimum distance in which vessels can interact with eachother at the launch pad and runway")]
        public float SafetyBubbleDistance { get; set; } = 100.0f;
        
        [XmlComment(Value = "Specify the warp Type. Values: None, Subspace, Master")]
        public WarpMode WarpMode { get; set; } = WarpMode.Subspace;

        [XmlComment(Value = "Username of the player who control the warp if WarpMode is set to MASTER")]
        public string WarpMaster { get; set; } = "";


        
        [XmlComment(Value = "Heartbeat interval in Ms")]
        public int HearbeatMsInterval { get; set; } = 1000;

        [XmlComment(Value = "Connection timeout in Ms")]
        public int ConnectionMsTimeout { get; set; } = 30000;

        [XmlComment(Value = "Interval in Ms at wich the client will send POSITION updates of his vessel when other players are NEARBY. " +
                     "Decrease it if your clients have good network connection and you plan to do dogfights, although in that case consider using interpolation aswell")]
        public int VesselUpdatesSendMsInterval { get; set; } = 80;

        [XmlComment(Value = "Interval in Ms at wich the client will send POSITION updates for vessels that are uncontrolled and nearby him. " +
                            "This interval is also applied used to send position updates of HIS OWN vessel when NOBODY is around")]
        public int SecondaryVesselUpdatesSendMsInterval { get; set; } = 500;

        [XmlComment(Value = "Interval in ms at wich users will check the controlled and close uncontrolled vessel and sync the parts that have changes " +
                            "(ladders that extend or shields that open) to the server. " +
                            "Caution! Puting a very low value could make clients with slow computers to lag a lot!")]
        public int VesselPartsSyncMsInterval { get; set; } = 500;

        [XmlComment(Value = "Relay system mode. Dictionary uses more RAM but it's faster. DataBase use disk space instead but it's slower. Values: Dictionary, DataBase")]
        public RelaySystemMode RelaySystemMode { get; set; } = RelaySystemMode.Dictionary;

        [XmlComment(Value = "Interval for saving POSITION updates IN THE SERVER so they are later sent to the OTHER players in the past. " +
                            "Lower number => smoother movement but as you're saving more position updates, then more memory will be required")]
        public int RelaySaveIntervalMs { get; set; } = 1000;

        [XmlComment(Value = "Send/Receive tick clock. Keep this value low but at least above 2ms to avoid extreme CPU usage.")]
        public int SendReceiveThreadTickMs { get; set; } = 5;

        [XmlComment(Value = "Main thread polling in ms. Keep this value low but at least above 2ms to avoid extreme CPU usage.")]
        public int MainTimeTick { get; set; } = 5;


        [XmlComment(Value = "Interval in ms at wich internal LMP structures (Subspaces, Vessels, Scenario files, ...) will be backed up to a file")]
        public int BackupIntervalMs { get; set; } = 30000;
    }
}
