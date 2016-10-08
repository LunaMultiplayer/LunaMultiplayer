using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Server;

namespace LunaCommon.Message
{
    /// <summary>
    ///     Class for deserialization of the master server messages that you've received
    /// </summary>
    public class MasterServerMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ServerMessageType);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compress">Compress the messages or not</param>
        public MasterServerMessageFactory(bool compress) : base(compress)
        {
            MessageDictionary.Add((uint)ServerMessageType.HANDSHAKE, new HandshakeSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.SETTINGS, new SetingsSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.CHAT, new ChatSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.PLAYER_STATUS, new PlayerStatusSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.PLAYER_COLOR, new PlayerColorSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.PLAYER_CONNECTION, new PlayerConnectionSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.SCENARIO, new ScenarioSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.KERBAL, new KerbalSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.VESSEL, new VesselSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.CRAFT_LIBRARY, new CraftLibrarySrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.FLAG, new FlagSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.SYNC_TIME, new SyncTimeSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.MOTD, new MotdSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.WARP, new WarpSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.ADMIN, new AdminSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.LOCK, new LockSrvMsg());
            MessageDictionary.Add((uint)ServerMessageType.MOD, new ModSrvMsg());
        }
    }
}