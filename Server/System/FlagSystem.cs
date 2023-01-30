using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.System
{
    public class FlagSystem
    {
        private static readonly Regex ValidationRegex = new Regex(@"^[-_a-zA-Z0-9/]+$");
        public static string FlagPath => Path.Combine(ServerContext.UniverseDirectory, "Flags");

        public static void HandleFlagDataMessage(ClientStructure client, FlagDataMsgData message)
        {
            if (!ValidationRegex.IsMatch(message.Flag.FlagName))
            {
                LunaLog.Warning($"Cannot save flag {message.Flag.FlagName} from {client.PlayerName} as it's flag name has invalid characters");
                return;
            }

            var playerFlagPath = Path.Combine(FlagPath, client.PlayerName);
            if (!FileHandler.FolderExists(playerFlagPath))
                FileHandler.FolderCreate(playerFlagPath);

            LunaLog.Debug($"Saving flag {message.Flag.FlagName} from {client.PlayerName}");
            var newFileName = $"{message.Flag.FlagName.Replace('/', '$')}.png";
            FileHandler.WriteToFile(Path.Combine(playerFlagPath, newFileName), message.Flag.FlagData, message.Flag.NumBytes);

            MessageQueuer.SendToAllClients<FlagSrvMsg>(message);
        }

        public static void HandleFlagListRequestMessage(ClientStructure client)
        {
            var flagList = new Dictionary<string, FlagInfo>();
            var serverFlags = FileHandler.GetFilesInPath(FlagPath, SearchOption.AllDirectories);
            foreach (var serverFlag in serverFlags)
            {
                var flagName = Path.GetFileNameWithoutExtension(serverFlag)?.Replace('$', '/');

                //2 playes have the same flag name so in this case we only send the first one
                if (string.IsNullOrEmpty(flagName) || flagList.ContainsKey(flagName)) continue;

                var flagOwner = Path.GetDirectoryName(serverFlag);
                if (flagOwner == null) continue;

                var flagData = File.ReadAllBytes(serverFlag);

                flagList.Add(flagName, new FlagInfo
                {
                    Owner = flagOwner,
                    FlagData = flagData,
                    FlagName = flagName,
                    NumBytes = flagData.Length
                });
            }

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<FlagListResponseMsgData>();
            msgData.FlagFiles = flagList.Values.ToArray();
            msgData.FlagCount = msgData.FlagFiles.Length;

            MessageQueuer.SendToClient<FlagSrvMsg>(client, msgData);
        }
    }
}