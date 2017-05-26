namespace LunaCommon.Message.Types
{
    public enum ChatMessageType
    {
        ListRequest = 0,
        ListReply = 1,
        Join = 2,
        Leave = 3,
        ChannelMessage = 4,
        PrivateMessage = 5,
        ConsoleMessage = 6
    }
}