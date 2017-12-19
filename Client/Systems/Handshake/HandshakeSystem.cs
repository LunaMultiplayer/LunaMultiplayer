using LunaClient.Base;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeSystem : MessageSystem<HandshakeSystem, HandshakeMessageSender, HandshakeMessageHandler>
    {
        public override string SystemName { get; } = nameof(HandshakeSystem);
    }
}