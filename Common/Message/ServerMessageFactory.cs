using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Server;
using System;

namespace LunaCommon.Message
{
    /// <summary>
    ///     Class for deserialization of the server messages that you've received
    /// </summary>
    public class ServerMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ServerMessageType);
        
        public ServerMessageFactory()
        {
            MessageDictionary.Add((uint)ServerMessageType.Handshake, typeof(HandshakeSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Settings, typeof(SetingsSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Chat, typeof(ChatSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.PlayerStatus, typeof(PlayerStatusSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.PlayerColor, typeof(PlayerColorSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.PlayerConnection, typeof(PlayerConnectionSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Scenario, typeof(ScenarioSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Kerbal, typeof(KerbalSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Vessel, typeof(VesselSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.CraftLibrary, typeof(CraftLibrarySrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Flag, typeof(FlagSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.SyncTime, typeof(SyncTimeSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Motd, typeof(MotdSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Warp, typeof(WarpSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Admin, typeof(AdminSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Lock, typeof(LockSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Mod, typeof(ModSrvMsg));
            MessageDictionary.Add((uint)ServerMessageType.Groups, typeof(GroupSrvMsg));
        }
    }
}