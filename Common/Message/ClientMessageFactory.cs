using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client;
using System;

namespace LunaCommon.Message
{
    /// <summary>
    /// Class for deserialization of the client messages that you've received
    /// </summary>
    public class ClientMessageFactory : FactoryBase
    {
        protected override Type HandledMessageTypes { get; } = typeof(ClientMessageType);
        
        public ClientMessageFactory()
        {
            MessageDictionary.Add((uint)ClientMessageType.Handshake, typeof(HandshakeCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Chat, typeof(ChatCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.PlayerStatus, typeof(PlayerStatusCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.PlayerColor, typeof(PlayerColorCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Scenario, typeof(ScenarioCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Kerbal, typeof(KerbalCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Settings, typeof(SettingsCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Vessel, typeof(VesselCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.CraftLibrary, typeof(CraftLibraryCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Flag, typeof(FlagCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Motd, typeof(MotdCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Warp, typeof(WarpCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Lock, typeof(LockCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Mod, typeof(ModCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Admin, typeof(AdminCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Groups, typeof(GroupCliMsg));
            MessageDictionary.Add((uint)ClientMessageType.Facility, typeof(FacilityCliMsg));
        }
    }
}