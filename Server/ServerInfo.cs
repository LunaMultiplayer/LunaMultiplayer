using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using LunaCommon;
using LunaServer.Context;
using LunaServer.Settings;
using LunaServer.System;

namespace LunaServer
{
    [DataContract]
    internal class ServerInfo
    {
        [DataMember] public bool Cheats;

        [DataMember] public string GameMode;

        [DataMember] public long LastPlayerActivity;

        [DataMember] public int MaxPlayers;

        [DataMember] public int ModControl;

        [DataMember] public string ModControlSha;

        [DataMember] public int PlayerCount;

        [DataMember] public string Players;

        [DataMember] public int Port;

        [DataMember] public string ServerName;

        [DataMember] public long UniverseSize;

        [DataMember] public string Version;

        [DataMember] public string WarpMode;

        public ServerInfo(SettingsStore settings)
        {
            ServerName = settings.ServerName;
            Version = VersionInfo.VersionNumber;
            PlayerCount = ServerContext.PlayerCount;
            Players = ServerContext.Players;
            MaxPlayers = settings.MaxPlayers;
            GameMode = settings.GameMode.ToString();
            WarpMode = settings.WarpMode.ToString();
            Port = settings.Port;
            ModControl = (int) settings.ModControl;
            Cheats = settings.Cheats;
            UniverseSize = Universe.GetUniverseSize();
            LastPlayerActivity = GetLastPlayerActivity();
            ModControlSha = ModFileSystem.GetModControlSha();
        }

        public string GetJson()
        {
            var serializer = new DataContractJsonSerializer(typeof(ServerInfo));

            var outStream = new MemoryStream();
            serializer.WriteObject(outStream, this);

            outStream.Position = 0;

            var sr = new StreamReader(outStream);

            return sr.ReadToEnd();
        }

        //Get last disconnect time
        private static long GetLastPlayerActivity()
        {
            return ServerContext.PlayerCount > 0
                ? 0
                : (ServerContext.ServerClock.ElapsedMilliseconds - ServerContext.LastPlayerActivity)/1000;
        }
    }
}