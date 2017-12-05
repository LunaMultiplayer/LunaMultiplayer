using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Plugin;
using LMP.Server.Server;
using LMP.Server.System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Server;
using System.IO;

namespace LMP.Server.Message.ReceiveHandlers
{
    public partial class HandshakeHandler
    {
        private string Reason { get; set; }

        public void HandleHandshakeResponse(ClientStructure client, HandshakeResponseMsgData data)
        {
            var valid = CheckServerFull(client);
            valid &= valid && CheckUsernameLength(client, data.PlayerName);
            valid &= valid && CheckUsernameCharacters(client, data.PlayerName);
            valid &= valid && CheckWhitelist(client, data.PlayerName);
            valid &= valid && CheckPlayerIsAlreadyConnected(client, data.PlayerName);
            valid &= valid && CheckUsernameIsReserved(client, data.PlayerName);
            valid &= valid && CheckPlayerIsBanned(client, data.PlayerName, client.Endpoint.Address.ToString(), data.PublicKey);
            valid &= valid && CheckKey(client, data.PlayerName, data.PublicKey, data.ChallengeSignature);

            if (!valid)
            {
                LunaLog.Normal($"Client {data.PlayerName} failed to handshake: {Reason}. Disconnecting");
                client.DisconnectClient = true;
                ClientConnectionHandler.DisconnectClient(client, Reason);
            }
            else
            {
                client.PlayerName = data.PlayerName;
                client.PublicKey = data.PublicKey;
                client.Authenticated = true;

                LmpPluginHandler.FireOnClientAuthenticated(client);

                LunaLog.Normal($"Client {data.PlayerName} handshook successfully, Version: {data.Version}");

                CreatePlayerScenarioFiles(client, data.PlayerName);

                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.HandshookSuccessfully, "success");

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<PlayerConnectionJoinMsgData>();
                msgData.PlayerName = client.PlayerName;

                MessageQueuer.RelayMessage<PlayerConnectionSrvMsg>(client, msgData);

                LunaLog.Debug($"Online Players: {ServerContext.PlayerCount}, connected: {ClientRetriever.GetClients().Length}");
            }
        }

        private static void CreatePlayerScenarioFiles(ClientStructure client, string playerName)
        {
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Scenarios", client.PlayerName)))
            {
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Scenarios", client.PlayerName));
                foreach (var file in FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Scenarios", "Initial")))
                {
                    var fileName = Path.GetFileName(file);
                    if (!string.IsNullOrEmpty(fileName))
                        FileHandler.FileCopy(file, Path.Combine(ServerContext.UniverseDirectory, "Scenarios", playerName, fileName));
                }
            }
        }
    }
}