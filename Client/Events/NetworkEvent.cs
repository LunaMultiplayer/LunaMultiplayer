using LunaCommon.Enums;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class NetworkEvent
    {
        public static EventData<ClientState> onNetworkStatusChanged { get; } = new EventData<ClientState>("onNetworkStatusChanged");
    }
}
