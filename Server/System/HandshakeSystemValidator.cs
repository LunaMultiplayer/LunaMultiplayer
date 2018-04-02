using LunaCommon.Enums;
using Server.Client;
using Server.Command.CombinedCommand;
using Server.Settings;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.System
{
    public partial class HandshakeSystem
    {
        private bool CheckUsernameLength(ClientStructure client, string username)
        {
            if (username.Length > GeneralSettings.SettingsStore.MaxUsernameLength)
            {
                Reason = $"Username too long. Max chars: {GeneralSettings.SettingsStore.MaxUsernameLength}";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }

            if (username.Length <= 0)
            {
                Reason = "Username too short. Min chars: 1";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
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

        private bool CheckPlayerIsBanned(ClientStructure client, string uniqueId)
        {
            if (BanCommands.RetrieveBannedPlayers().Contains(uniqueId))
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
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }

        private bool CheckPlayerIsAlreadyConnected(ClientStructure client, string playerName)
        {
            var existingClient = ClientRetriever.GetClientByName(playerName);
            if (existingClient != null)
            {
                Reason = "Username already taken";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }

        private bool CheckUsernameCharacters(ClientStructure client, string playerName)
        {
            var regex = new Regex(@"^[-_a-zA-Z0-9]+$"); // Regex to only allow alphanumeric, dashes and underscore
            if (!regex.IsMatch(playerName))
            {
                Reason = "Invalid username characters (only A-Z, a-z, numbers, - and _)";
                HandshakeSystemSender.SendHandshakeReply(client, HandshakeReply.InvalidPlayername, Reason);
                return false;
            }
            return true;
        }
    }
}
