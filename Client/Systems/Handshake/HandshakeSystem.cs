using LunaClient.Base;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeSystem : MessageSystem<HandshakeSystem, HandshakeMessageSender, HandshakeMessageHandler>
    {
        public override string SystemName { get; } = nameof(HandshakeSystem);

        protected override bool AlwaysEnabled => true;
        protected override bool ProcessMessagesInUnityThread => false;
    }
}
