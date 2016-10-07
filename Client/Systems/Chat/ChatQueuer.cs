using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaClient.Windows.Chat;

namespace LunaClient.Systems.Chat
{
    public class ChatQueuer : SubSystem<ChatSystem>
    {
        private static ChatWindow Screen => ChatWindow.Singleton;

        public ConcurrentQueue<string> DisconnectingPlayers { get; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<JoinLeaveMessage> NewJoinMessages { get; } = new ConcurrentQueue<JoinLeaveMessage>();
        public ConcurrentQueue<JoinLeaveMessage> NewLeaveMessages { get; } = new ConcurrentQueue<JoinLeaveMessage>();
        public ConcurrentQueue<ChannelEntry> NewChannelMessages { get; } = new ConcurrentQueue<ChannelEntry>();
        public ConcurrentQueue<PrivateEntry> NewPrivateMessages { get; } = new ConcurrentQueue<PrivateEntry>();
        public ConcurrentQueue<ConsoleEntry> NewConsoleMessages { get; } = new ConcurrentQueue<ConsoleEntry>();

        public void QueueChatJoin(string playerName, string channelName)
        {
            var jlm = new JoinLeaveMessage
            {
                FromPlayer = playerName,
                Channel = channelName
            };
            NewJoinMessages.Enqueue(jlm);
        }

        public void QueueChatLeave(string playerName, string channelName)
        {
            var jlm = new JoinLeaveMessage
            {
                FromPlayer = playerName,
                Channel = channelName
            };
            NewLeaveMessages.Enqueue(jlm);
        }

        public void QueueChannelMessage(string fromPlayer, string channelName, string channelMessage)
        {
            var ce = new ChannelEntry
            {
                FromPlayer = fromPlayer,
                Channel = channelName,
                Message = channelMessage
            };
            NewChannelMessages.Enqueue(ce);
            if (!Screen.Display)
            {
                if (ce.FromPlayer != SettingsSystem.ServerSettings.ConsoleIdentifier)
                    System.ChatButtonHighlighted = true;
                if (ce.Channel != "")
                    ScreenMessages.PostScreenMessage(ce.FromPlayer + " -> #" + ce.Channel + ": " + ce.Message, 5f,
                        ScreenMessageStyle.UPPER_LEFT);
                else
                    ScreenMessages.PostScreenMessage(ce.FromPlayer + " -> #Global : " + ce.Message, 5f,
                        ScreenMessageStyle.UPPER_LEFT);
            }
        }

        public void QueuePrivateMessage(string fromPlayer, string toPlayer, string privateMessage)
        {
            var pe = new PrivateEntry
            {
                FromPlayer = fromPlayer,
                ToPlayer = toPlayer,
                Message = privateMessage
            };
            NewPrivateMessages.Enqueue(pe);
            if (!Screen.Display)
            {
                System.ChatButtonHighlighted = true;
                if (pe.FromPlayer != SettingsSystem.CurrentSettings.PlayerName)
                    ScreenMessages.PostScreenMessage(pe.FromPlayer + " -> @" + pe.ToPlayer + ": " + pe.Message, 5f,
                        ScreenMessageStyle.UPPER_LEFT);
            }
        }

        public void QueueRemovePlayer(string playerName)
        {
            DisconnectingPlayers.Enqueue(playerName);
        }

        public void QueueSystemMessage(string message)
        {
            var ce = new ConsoleEntry {Message = message};
            NewConsoleMessages.Enqueue(ce);
        }
    }
}