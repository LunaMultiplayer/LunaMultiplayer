using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using LunaCommon.Enums;
using LunaServer.Client;
using LunaServer.Command.CombinedCommand;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Settings;
using LunaServer.System;

namespace LunaServer.Message.ReceiveHandlers
{
    public partial class HandshakeHandler
    {
        private bool CheckUsernameLength(ClientStructure client, string username)
        {
            if (username.Length > 10)
            {
                Reason = "User too long. Max: 10";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.SERVER_FULL, Reason);
                return false;
            }
            return true;
        }

        private bool CheckServerFull(ClientStructure client)
        {
            if (ClientRetriever.GetActiveClientCount() >= GeneralSettings.SettingsStore.MaxPlayers)
            {
                Reason = "Server full";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.SERVER_FULL, Reason);
                return false;
            }
            return true;
        }

        private bool CheckWhitelist(ClientStructure client, string playerName)
        {
            if (GeneralSettings.SettingsStore.Whitelisted && !WhitelistCommands.Retrieve().Contains(playerName))
            {
                Reason = "Not on whitelist";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.NOT_WHITELISTED, Reason);
                return false;
            }
            return true;
        }

        private bool CheckPlayerIsBanned(ClientStructure client, string playerName, string ipAddress, string publicKey)
        {
            if (BanCommands.RetrieveBannedUsernames().Contains(playerName) ||
                BanCommands.RetrieveBannedIps().Contains(ipAddress) ||
                BanCommands.RetrieveBannedKeys().Contains(publicKey))
            {
                Reason = "Banned";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.PLAYER_BANNED, Reason);
                return false;
            }
            return true;
        }

        private bool CheckUsernameIsReserved(ClientStructure client, string playerName)
        {
            if ((playerName == "Initial") || (playerName == GeneralSettings.SettingsStore.ConsoleIdentifier))
            {
                Reason = "Using reserved name";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.RESERVED_NAME, Reason);
                return false;
            }
            return true;
        }

        private bool CheckPlayerIsAlreadyConnected(ClientStructure client, string playerName)
        {
            var existingClient = ClientRetriever.GetClientByName(playerName);
            if (existingClient != null)
            {
                Reason = "Username already connected";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.ALREADY_CONNECTED, Reason);
                return false;
            }
            return true;
        }

        private bool CheckUsernameCharacters(ClientStructure client, string playerName)
        {
            var regex = new Regex(@"^[-_a-zA-Z0-9]+$"); // Regex to only allow alphanumeric, dashes and underscore
            if (!regex.IsMatch(playerName))
            {
                Reason = "Invalid username characters";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.INVALID_PLAYERNAME, Reason);
                return false;
            }
            return true;
        }

        private bool CheckKey(ClientStructure client, string playerName, string playerPublicKey,
            byte[] playerChallangeSignature)
        {
            //Check the client matches any database entry
            var storedPlayerFile = Path.Combine(ServerContext.UniverseDirectory, "Players", playerName + ".txt");
            if (FileHandler.FileExists(storedPlayerFile))
            {
                var storedPlayerPublicKey = FileHandler.ReadFileText(storedPlayerFile);
                if (playerPublicKey != storedPlayerPublicKey)
                {
                    Reason = "Invalid key. Username already taken";
                    HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.INVALID_KEY, Reason);
                    return false;
                }
                using (var rsa = new RSACryptoServiceProvider(1024))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(playerPublicKey);
                    var result = rsa.VerifyData(client.Challange, CryptoConfig.CreateFromName("SHA256"),
                        playerChallangeSignature);
                    if (!result)
                    {
                        Reason = "Public/priv key mismatch";
                        HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.INVALID_KEY, Reason);
                        return false;
                    }
                }
            }
            else
            {
                try
                {
                    FileHandler.WriteToFile(storedPlayerFile, playerPublicKey);
                    LunaLog.Debug($"Client {playerName} registered!");
                }
                catch
                {
                    Reason = "Invalid username";
                    HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.INVALID_PLAYERNAME, Reason);
                    return false;
                }
            }
            return true;
        }
    }
}