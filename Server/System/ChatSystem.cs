using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Command.CombinedCommand;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.System
{
    public abstract class ChatSystem
    {
        private static readonly object ChatLock = new object();

        protected static ConcurrentDictionary<string, List<string>> PlayerChatChannels { get; set; } =
            new ConcurrentDictionary<string, List<string>>();

        protected static ClientStructure[] GetClientsInChannel(string channel)
        {
            return PlayerChatChannels.Where(p => p.Value.Contains(channel))
                .Select(p => ClientRetriever.GetClientByName(p.Key)).Where(c => c != null).ToArray();
        }

        public static void SendPlayerChatChannels(ClientStructure client)
        {
            var channels = PlayerChatChannels.Select(v => new KeyValuePair<string, string[]>(v.Key, v.Value.ToArray())).ToArray();
            MessageQueuer.SendToClient<ChatSrvMsg>(client, new ChatListReplyMsgData { PlayerChannels = channels });
        }

        public static void RemovePlayer(string playerName)
        {
            if (PlayerChatChannels.ContainsKey(playerName))
            {
                List<string> chatChannels;
                PlayerChatChannels.TryRemove(playerName, out chatChannels);
            }
        }

        public static void Reset()
        {
            PlayerChatChannels.Clear();
        }

        public static void SendConsoleMessageToAdmins(string text)
        {
            var admins = AdminCommands.Retrieve();
            foreach (var client in ClientRetriever.GetAuthenticatedClients().Where(c => admins.Contains(c.PlayerName)))
            {
                var messageData = new ChatConsoleMsgData
                {
                    From = GeneralSettings.SettingsStore.ConsoleIdentifier,
                    Message = text
                };

                MessageQueuer.SendToClient<ChatSrvMsg>(client, messageData);
            }
        }

        protected static void AddChannelToPlayer(string playerName, string channel)
        {
            if (!PlayerChatChannels.ContainsKey(playerName))
                PlayerChatChannels.TryAdd(playerName, new List<string>());

            //Lists are not  thread safe so we must lock
            lock (ChatLock)
            {
                if (!PlayerChatChannels[playerName].Contains(channel))
                    PlayerChatChannels[playerName].Add(channel);
            }
        }

        protected static void RemoveChannelFromPlayer(string playerName, string channel)
        {
            //Lists are not  thread safe so we must lock
            lock (ChatLock)
            {
                if (PlayerChatChannels.ContainsKey(playerName) && PlayerChatChannels[playerName].Contains(channel))
                    PlayerChatChannels[playerName].Remove(channel);
            }
        }
    }
}