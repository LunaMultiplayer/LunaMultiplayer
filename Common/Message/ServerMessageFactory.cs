using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Server;

namespace LunaCommon.Message
{
    /// <summary>
    ///     Class for deserialization of the server messages that you've received
    /// </summary>
    public class ServerMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ServerMessageType);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compress">Compress the messages or not</param>
        public ServerMessageFactory(bool compress) : base(compress)
        {
            MessageDictionary.Add((uint)ServerMessageType.Handshake, new HandshakeSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Settings, new SetingsSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Chat, new ChatSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.PlayerStatus, new PlayerStatusSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.PlayerColor, new PlayerColorSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.PlayerConnection, new PlayerConnectionSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Scenario, new ScenarioSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Kerbal, new KerbalSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Vessel, new VesselSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.CraftLibrary, new CraftLibrarySrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Flag, new FlagSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.SyncTime, new SyncTimeSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Motd, new MotdSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Warp, new WarpSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Admin, new AdminSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Lock, new LockSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.Mod, new ModSrvMsg());
        }
    }
}