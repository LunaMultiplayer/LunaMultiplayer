using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaServer.Message.ReceiveHandlers
{
    public class CraftLibraryHandler
    {
        public static void SendCraftList(ClientStructure client)
        {
            var craftDirectory = Path.Combine(ServerContext.UniverseDirectory, "Crafts");
            if (!FileHandler.FolderExists(craftDirectory))
                FileHandler.FolderCreate(craftDirectory);
            var players = FileHandler.GetDirectoriesInPath(craftDirectory);
            for (var i = 0; i < players.Length; i++)
                players[i] = players[i].Substring(players[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryListReplyMsgData>();
            msgData.Players = players;
            
            var playerCrafts = new List<KeyValuePair<string, CraftListInfo>>();

            foreach (var player in players)
            {
                var playerPath = Path.Combine(craftDirectory, player);
                var vabPath = Path.Combine(playerPath, "VAB");
                var sphPath = Path.Combine(playerPath, "SPH");
                var subassemplyPath = Path.Combine(playerPath, "SUBASSEMBLY");
                var vabCraftNames = new List<string>();
                var sphCraftNames = new List<string>();
                var subassemblyCraftNames = new List<string>();

                var newPlayerCraft = new CraftListInfo
                {
                    SphExists = FileHandler.FolderExists(sphPath),
                    VabExists = FileHandler.FolderExists(vabPath),
                    SubassemblyExists = FileHandler.FolderExists(subassemplyPath)
                };

                if (newPlayerCraft.VabExists)
                    vabCraftNames.AddRange(
                        FileHandler.GetFilesInPath(vabPath).Select(Path.GetFileNameWithoutExtension));
                if (newPlayerCraft.VabExists)
                    sphCraftNames.AddRange(
                        FileHandler.GetFilesInPath(sphPath).Select(Path.GetFileNameWithoutExtension));
                if (newPlayerCraft.VabExists)
                    subassemblyCraftNames.AddRange(
                        FileHandler.GetFilesInPath(subassemplyPath).Select(Path.GetFileNameWithoutExtension));

                newPlayerCraft.VabCraftNames = vabCraftNames.ToArray();
                newPlayerCraft.SphCraftNames = sphCraftNames.ToArray();
                newPlayerCraft.SubassemblyCraftNames = subassemblyCraftNames.ToArray();

                playerCrafts.Add(new KeyValuePair<string, CraftListInfo>(player, newPlayerCraft));
            }

            msgData.PlayerCrafts = playerCrafts.ToArray();
            MessageQueuer.SendToClient<CraftLibrarySrvMsg>(client, msgData);
        }

        public void HandleDeleteFileMessage(ClientStructure client, CraftLibraryDeleteMsgData message)
        {
            var playerPath = Path.Combine(Path.Combine(ServerContext.UniverseDirectory, "Crafts"), message.PlayerName);
            var typePath = Path.Combine(playerPath, message.CraftType.ToString());
            var craftFile = Path.Combine(typePath, $"{message.CraftName}.craft");
            if (FileHandler.FolderExists(playerPath) && FileHandler.FolderExists(typePath))
            {
                Universe.RemoveFromUniverse(craftFile);
                LunaLog.Debug($"Removing {message.CraftName}, type: {message.CraftType} from {message.PlayerName}");
            }
            if (FileHandler.FolderExists(playerPath) && FileHandler.GetFilesInPath(typePath).Length == 0)
                FileHandler.FolderDelete(typePath);
            if (FileHandler.GetDirectoriesInPath(playerPath).Length == 0)
                FileHandler.FolderDelete(playerPath);

            MessageQueuer.RelayMessage<CraftLibrarySrvMsg>(client, message);
        }

        public void HandleRequestFileMessage(ClientStructure client, CraftLibraryRequestMsgData message)
        {
            var playerPath = Path.Combine(Path.Combine(ServerContext.UniverseDirectory, "Crafts"), message.CraftOwner);
            var typePath = Path.Combine(playerPath, message.RequestedType.ToString());
            var craftFile = Path.Combine(typePath, $"{message.RequestedName}.craft");

            var hasCraft = FileHandler.FolderExists(playerPath) && FileHandler.FolderExists(typePath) &&
                           FileHandler.FileExists(craftFile);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryRespondMsgData>();
            msgData.CraftOwner = message.CraftOwner;
            msgData.RequestedType = message.RequestedType;
            msgData.HasCraft = hasCraft;
            
            if (hasCraft)
                msgData.CraftData = FileHandler.ReadFile(craftFile);

            MessageQueuer.SendToClient<CraftLibrarySrvMsg>(client, msgData);
        }

        public void HandleUploadFileMessage(ClientStructure client, CraftLibraryUploadMsgData message)
        {
            var playerPath = Path.Combine(Path.Combine(ServerContext.UniverseDirectory, "Crafts"),
                message.PlayerName);
            if (!FileHandler.FolderExists(playerPath))
                FileHandler.FolderCreate(playerPath);
            var typePath = Path.Combine(playerPath, message.UploadType.ToString());
            if (!FileHandler.FolderExists(typePath))
                FileHandler.FolderCreate(typePath);
            var craftFile = Path.Combine(typePath, $"{message.UploadName}.craft");
            FileHandler.WriteToFile(craftFile, message.CraftData);
            LunaLog.Debug($"Saving {message.UploadName}, Type: {message.UploadType} from {message.PlayerName}");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryAddMsgData>();
            msgData.PlayerName = message.PlayerName;
            msgData.UploadName = message.UploadName;
            msgData.UploadType = message.UploadType;
            
            MessageQueuer.RelayMessage<CraftLibrarySrvMsg>(client, msgData);
        }
    }
}