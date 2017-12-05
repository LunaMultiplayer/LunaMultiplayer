using LMP.Server.Client;
using LMP.Server.Command;
using LMP.Server.Command.CombinedCommand;
using LMP.Server.Log;
using LMP.Server.Server;
using LMP.Server.Settings;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using System.Linq;

namespace LMP.Server.System
{
    public class ChatSystemReceiver : ChatSystem
    {
        public void HandleConsoleMessage(ClientStructure client, ChatConsoleMsgData message)
        {
            if (client.Authenticated && AdminCommands.Admins.Contains(client.PlayerName))
                CommandHandler.HandleServerInput(message.Message);
        }

        public void HandlePrivateMessage(ClientStructure client, ChatPrivateMsgData message)
        {
            if (message.To != GeneralSettings.SettingsStore.ConsoleIdentifier)
            {
                var findClient = ClientRetriever.GetClientByName(message.To);
                if (findClient != null)
                {
                    MessageQueuer.SendToClient<ChatSrvMsg>(client, message); //Send it to the sender
                    MessageQueuer.SendToClient<ChatSrvMsg>(findClient, message); //Send it to destination
                    LunaLog.ChatMessage($"{message.From} -> @{message.To}: {message.Text}");
                }
                else
                {
                    LunaLog.ChatMessage($"{message.From} -X-> @{message.To}: {message.Text}");
                }
            }
            else
            {
                //Send it to the sender only as we as server already received it
                MessageQueuer.SendToClient<ChatSrvMsg>(client, message);
                LunaLog.ChatMessage($"{message.From} -> @{message.To}: {message.Text}");
            }
        }

        public void HandleChannelMessage(ClientStructure client, ChatChannelMsgData message)
        {
            if (message.SendToAll)
            {
                //send it to the player and send it to the other players too
                MessageQueuer.SendToAllClients<ChatSrvMsg>(message);
                LunaLog.ChatMessage($"{message.From} -> #Global: {message.Text}");
            }
            else //Is a channel message
            {
                //send it to the player
                MessageQueuer.SendToClient<ChatSrvMsg>(client, message);

                foreach (var player in GetClientsInChannel(message.Channel))
                    MessageQueuer.SendToClient<ChatSrvMsg>(player, message);
                LunaLog.ChatMessage($"{message.From} -> #{message.Channel}: {message.Text}");
            }
        }

        public void HandleLeaveMessage(ClientStructure client, ChatLeaveMsgData message)
        {
            RemoveChannelFromPlayer(message.From, message.Channel);
            LunaLog.Debug($"{message.From} left channel: {message.Channel}");
            MessageQueuer.RelayMessage<ChatSrvMsg>(client, message);
        }

        public void HandleJoinMessage(ClientStructure client, ChatJoinMsgData message)
        {
            AddChannelToPlayer(message.From, message.Channel);
            LunaLog.Debug($"{message.From} joined channel: {message.Channel}");
            MessageQueuer.RelayMessage<ChatSrvMsg>(client, message);
        }
    }
}