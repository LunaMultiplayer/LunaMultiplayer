using LunaClient.Events.Base;
using LunaCommon.Enums;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class NetworkEvent : LmpBaseEvent
    {
        public static EventData<ClientState> onNetworkStatusChanged;
    }
}
