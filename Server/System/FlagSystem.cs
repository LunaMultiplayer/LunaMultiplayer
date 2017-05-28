using LunaCommon;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace LunaServer.System
{
    public class FlagSyncMsgSender
    {
        private static string FlagPath => Path.Combine(ServerContext.UniverseDirectory, "Flags");

        public static void HandleUploadFlagMessage(ClientStructure client, FlagUploadMsgData message)
        {
            var playerFlagPath = Path.Combine(FlagPath, message.PlayerName);
            if (!FileHandler.FolderExists(playerFlagPath))
                FileHandler.FolderCreate(playerFlagPath);
            LunaLog.Debug($"Saving flag {message.FlagName} from {message.PlayerName}");
            FileHandler.WriteToFile(Path.Combine(playerFlagPath, message.FlagName), message.FlagData);

            MessageQueuer.RelayMessage<FlagSrvMsg>(client, message);
        }

        public static void HandleDeleteFlagMessage(ClientStructure client, FlagDeleteMsgData message)
        {
            var playerFlagPath = Path.Combine(FlagPath, message.PlayerName);
            if (FileHandler.FolderExists(playerFlagPath))
            {
                var flagFile = Path.Combine(playerFlagPath, message.FlagName);
                if (FileHandler.FileExists(flagFile))
                    FileHandler.FileDelete(flagFile);
                if (FileHandler.GetFilesInPath(playerFlagPath).Length == 0)
                    FileHandler.FolderDelete(playerFlagPath);
            }
            MessageQueuer.RelayMessage<FlagSrvMsg>(client, message);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static void HandleListFlagMessage(ClientStructure client, FlagListMsgData message)
        {
            //Send the list back
            var serverFlagFileNames = new List<string>();
            var serverFlagOwners = new List<string>();
            var serverFlagShaSums = new List<string>();
            var serverFlags = FileHandler.GetFilesInPath(FlagPath, SearchOption.AllDirectories);
            foreach (var serverFlag in serverFlags)
            {
                var trimmedName = Path.GetFileName(serverFlag);
                var flagOwnerPath = Path.GetDirectoryName(serverFlag);

                var flagOwner = flagOwnerPath.Substring(Path.GetDirectoryName(flagOwnerPath).Length + 1);
                var isMatched = false;
                var shaDifferent = false;
                for (var i = 0; i < message.FlagFileNames.Length; i++)
                    if (message.FlagFileNames[i].ToLower() == trimmedName.ToLower())
                    {
                        isMatched = true;
                        shaDifferent = Common.CalculateSha256Hash(serverFlag) != message.FlagShaSums[i];
                    }
                if (!isMatched || shaDifferent)
                    if (flagOwner == client.PlayerName)
                    {
                        LunaLog.Debug($"Deleting flag {trimmedName}");
                        FileHandler.FileDelete(serverFlag);

                        MessageQueuer.RelayMessage<FlagSrvMsg>(client,
                            new FlagDeleteMsgData { FlagName = trimmedName });

                        if (FileHandler.GetFilesInPath(flagOwnerPath).Length == 0)
                            FileHandler.FolderDelete(flagOwnerPath);
                    }
                    else
                    {
                        LunaLog.Debug($"Sending flag {serverFlag} from {flagOwner} to {client.PlayerName}");

                        var newMessageData = new FlagDataMsgData
                        {
                            FlagName = trimmedName,
                            OwnerPlayerName = flagOwner,
                            FlagData = FileHandler.ReadFile(serverFlag)
                        };

                        MessageQueuer.SendToClient<FlagSrvMsg>(client, newMessageData);
                    }
                //Don't tell the client we have a different copy of the flag so it is reuploaded
                if (FileHandler.FileExists(serverFlag))
                {
                    serverFlagFileNames.Add(trimmedName);
                    serverFlagOwners.Add(flagOwner);
                    serverFlagShaSums.Add(Common.CalculateSha256Hash(serverFlag));
                }
            }

            var listMessageData = new FlagListMsgData
            {
                FlagFileNames = serverFlagFileNames.ToArray(),
                FlagOwners = serverFlagOwners.ToArray(),
                FlagShaSums = serverFlagShaSums.ToArray()
            };

            MessageQueuer.SendToClient<FlagSrvMsg>(client, listMessageData);
        }
    }
}