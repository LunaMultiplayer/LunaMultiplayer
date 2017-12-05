using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using LunaCommon.Enums;
using Server.Client;
using Server.Command.CombinedCommand;
using Server.Context;
using Server.Log;
using Server.Settings;
using Server.System;

namespace Server.Message.ReceiveHandlers
{
    public partial class HandshakeHandler
    {
        private bool CheckUsernameLength(ClientStructure client, string username)
        {
            if (username.Length > 10)
            {
                Reason = "User too long. Max: 10";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.ServerFull, Reason);
                return false;
            }
            return true;
        }

        private bool CheckServerFull(ClientStructure client)
        {
            if (ClientRetriever.GetActiveClientCount() >= GeneralSettings.SettingsStore.MaxPlayers)
            {
                Reason = "Server full";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.ServerFull, Reason);
                return false;
            }
            return true;
        }

        private bool CheckWhitelist(ClientStructure client, string playerName)
        {
            if (GeneralSettings.SettingsStore.Whitelisted && !WhitelistCommands.Retrieve().Contains(playerName))
            {
                Reason = "Not on whitelist";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.NotWhitelisted, Reason);
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
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.PlayerBanned, Reason);
                return false;
            }
            return true;
        }

        private bool CheckUsernameIsReserved(ClientStructure client, string playerName)
        {
            if (playerName == "Initial" || playerName == GeneralSettings.SettingsStore.ConsoleIdentifier)
            {
                Reason = "Using reserved name";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.ReservedName, Reason);
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
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.AlreadyConnected, Reason);
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
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }

        private bool CheckKey(ClientStructure client, string playerName, string playerPublicKey,
            byte[] playerChallangeSignature)
        {
            //Check the client matches any database entry
            var storedPlayerFile = Path.Combine(ServerContext.UniverseDirectory, "Players", $"{playerName}.txt");
            if (FileHandler.FileExists(storedPlayerFile))
            {
                var storedPlayerPublicKey = FileHandler.ReadFileText(storedPlayerFile);
                if (playerPublicKey != storedPlayerPublicKey)
                {
                    Reason = "Invalid key. Username was already taken and used in the past";
                    HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidKey, Reason);
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
                        HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidKey, Reason);
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
                    HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                    return false;
                }
            }
            return true;
        }
    }
}