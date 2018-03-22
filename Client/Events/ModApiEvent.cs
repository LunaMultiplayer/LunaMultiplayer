// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class ModApiEvent
    {
        public static EventData<string, byte[]> onModMessageReceived = new EventData<string, byte[]>("onModMessageReceived");
    }
}
