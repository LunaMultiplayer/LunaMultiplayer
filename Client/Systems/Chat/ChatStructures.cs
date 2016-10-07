using System;

namespace LunaClient.Systems.Chat
{
    public class ChatCommand : IComparable
    {
        public ChatCommand(string name, Action<string> func, string description)
        {
            Name = name;
            Func = func;
            Description = description;
        }

        public string Name { get; }
        public Action<string> Func { get; }
        public string Description { get; }

        public int CompareTo(object obj)
        {
            var cmd = obj as ChatCommand;
            return string.Compare(Name, cmd?.Name, StringComparison.Ordinal);
        }
    }

    public class ChannelEntry
    {
        public string FromPlayer { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
    }

    public class PrivateEntry
    {
        public string FromPlayer { get; set; }
        public string ToPlayer { get; set; }
        public string Message { get; set; }
    }

    public class JoinLeaveMessage
    {
        public string FromPlayer { get; set; }
        public string Channel { get; set; }
    }

    public class ConsoleEntry
    {
        public string Message { get; set; }
    }
}