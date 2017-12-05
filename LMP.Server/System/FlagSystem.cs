using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Server;
using LunaCommon.Flag;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LMP.Server.System
{
    public class FlagSyncMsgSender
    {
        private static string FlagPath => Path.Combine(ServerContext.UniverseDirectory, "Flags");

        public static void HandleFlagDataMessage(ClientStructure client, FlagDataMsgData message)
        {
            var playerFlagPath = Path.Combine(FlagPath, client.PlayerName);
            if (!FileHandler.FolderExists(playerFlagPath))
                FileHandler.FolderCreate(playerFlagPath);
            LunaLog.Debug($"Saving flag {message.Flag.FlagName} from {client.PlayerName}");
            FileHandler.WriteToFile(Path.Combine(playerFlagPath, message.Flag.FlagName), message.Flag.FlagData);

            MessageQueuer.SendToAllClients<FlagSrvMsg>(message);
        }

        public static void HandleFlagDeleteMessage(ClientStructure client, FlagDeleteMsgData message)
        {
            var playerFlagPath = Path.Combine(FlagPath, client.PlayerName);
            if (FileHandler.FolderExists(playerFlagPath))
            {
                var flagFile = Path.Combine(playerFlagPath, message.FlagName);
                if (FileHandler.FileExists(flagFile))
                    FileHandler.FileDelete(flagFile);
                if (FileHandler.GetFilesInPath(playerFlagPath).Length == 0)
                    FileHandler.FolderDelete(playerFlagPath);
            }
            MessageQueuer.SendToAllClients<FlagSrvMsg>(message);
        }

        public static void HandleFlagListRequestMessage(ClientStructure client)
        {
            var flagList = new Dictionary<string, FlagInfo>();
            var serverFlags = FileHandler.GetFilesInPath(FlagPath, SearchOption.AllDirectories);
            foreach (var serverFlag in serverFlags)
            {
                var flagName = Path.GetFileName(serverFlag);
                //2 playes have the same flag name so in this case we only send the first one
                if (flagName == null || flagList.ContainsKey(flagName)) continue;

                var flagOwner = Path.GetDirectoryName(serverFlag);
                if(flagOwner == null) continue;
                flagList.Add(flagName, new FlagInfo
                {
                    Owner = flagOwner,
                    FlagData = File.ReadAllBytes(serverFlag),
                    FlagName = flagName
                });
            }

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<FlagListResponseMsgData>();
            msgData.FlagFiles = flagList.Values.ToArray();

            MessageQueuer.SendToClient<FlagSrvMsg>(client, msgData);
        }
    }
}