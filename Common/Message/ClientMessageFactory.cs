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
            MessageDictionary.Add((uint)ClientMessageType.Handshake, new HandshakeCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Chat, new ChatCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.PlayerStatus, new PlayerStatusCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.PlayerColor, new PlayerColorCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Scenario, new ScenarioCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Kerbal, new KerbalCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Settings, new SettingsCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Vessel, new VesselCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.CraftLibrary, new CraftLibraryCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Flag, new FlagCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.SyncTime, new SyncTimeCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Motd, new MotdCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Warp, new WarpCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Lock, new LockCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Mod, new ModCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Admin, new AdminCliMsg());
            MessageDictionary.Add((uint)ClientMessageType.Groups, new GroupCliMsg());
        }
    }
}