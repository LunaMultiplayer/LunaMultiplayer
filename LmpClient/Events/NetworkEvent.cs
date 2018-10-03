using LmpClient.Events.Base;
using LmpCommon.Enums;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class NetworkEvent : LmpBaseEvent
    {
        public static EventData<ClientState> onNetworkStatusChanged;
    }
}
