namespace LunaClient.Systems.ModApi
{
    public delegate void MessageCallback(byte[] messageData);

    public class QueuedMessage
    {
        public string ModName { get; set; }
        public byte[] MessageData { get; set; }
    }
}