using LunaClient.Base;
using LunaClient.Windows;
using LunaClient.Windows.Chat;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Chat
{
    public class ChatQueuer : SubSystem<ChatSystem>
    {
        private static ChatWindow Screen => ChatWindow.Singleton;

        public ConcurrentQueue<string> DisconnectingPlayers { get; private set; } = new ConcurrentQueue<string>();
        public ConcurrentQueue<JoinLeaveMessage> NewJoinMessages { get; private set; } = new ConcurrentQueue<JoinLeaveMessage>();
        public ConcurrentQueue<JoinLeaveMessage> NewLeaveMessages { get; private set; } = new ConcurrentQueue<JoinLeaveMessage>();
        public ConcurrentQueue<ChannelEntry> NewChannelMessages { get; private set; } = new ConcurrentQueue<ChannelEntry>();
        public ConcurrentQueue<PrivateEntry> NewPrivateMessages { get; private set; } = new ConcurrentQueue<PrivateEntry>();
        public ConcurrentQueue<ConsoleEntry> NewConsoleMessages { get; private set; } = new ConcurrentQueue<ConsoleEntry>();

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
        }

        public void QueueRemovePlayer(string playerName)
        {
            DisconnectingPlayers.Enqueue(playerName);
        }

        public void QueueSystemMessage(string message)
        {
            var ce = new ConsoleEntry { Message = message };
            NewConsoleMessages.Enqueue(ce);
        }

        public void Clear()
        {
            DisconnectingPlayers = new ConcurrentQueue<string>();
            NewJoinMessages = new ConcurrentQueue<JoinLeaveMessage>();
            NewLeaveMessages = new ConcurrentQueue<JoinLeaveMessage>();
            NewChannelMessages = new ConcurrentQueue<ChannelEntry>();
            NewPrivateMessages = new ConcurrentQueue<PrivateEntry>();
            NewConsoleMessages = new ConcurrentQueue<ConsoleEntry>();
        }
    }
}