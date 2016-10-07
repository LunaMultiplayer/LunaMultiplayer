using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client;

namespace LunaCommon.Message
{
    /// <summary>
    ///     Class for deserialization of the client messages that you've received
    /// </summary>
    public class ClientMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ClientMessageType);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compress">Compress the messages or not</param>
        public ClientMessageFactory(bool compress) : base(compress)
        {
            MessageDictionary.Add((uint)ClientMessageType.HANDSHAKE, new HandshakeCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.CHAT, new ChatCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.PLAYER_STATUS, new PlayerStatusCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.PLAYER_COLOR, new PlayerColorCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.SCENARIO, new ScenarioCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.KERBAL, new KerbalCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.SETTINGS, new SettingsCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.VESSEL, new VesselCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.CRAFT_LIBRARY, new CraftLibraryCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.FLAG, new FlagCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.SYNC_TIME, new SyncTimeCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.MOTD, new MotdCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.WARP, new WarpCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.LOCK, new LockCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.MOD, new ModCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.ADMIN, new AdminCliMsg());
        }
    }
}