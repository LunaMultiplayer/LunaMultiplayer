using LunaCommon.Enums;
using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class GeneralSettingsDefinition
    {
        [XmlComment(Value = "Name of the server. Max 30 char")]
        public string ServerName { get; set; } = "Luna Server";

        [XmlComment(Value = "Description of the server. Max 200 char")]
        public string Description { get; set; } = "Luna Server Description";

        [XmlComment(Value = "Website text to display (discord, website, forum, etc). Max 15 char")]
        public string WebsiteText { get; set; } = "LMP Github Repo";

        [XmlComment(Value = "Actual website URL. Max 60 char")]
        public string Website { get; set; } = "https://lunamultiplayer.github.io";

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
        public GameDifficulty GameDifficulty { get; set; } = GameDifficulty.Easy;

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
    }
}
