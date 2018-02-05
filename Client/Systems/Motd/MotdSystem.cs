using LunaClient.Base;

namespace LunaClient.Systems.Motd
{
    public class MotdSystem : MessageSystem<MotdSystem, MotdMessageSender, MotdMessageHandler>
    {
        public string ServerMotd { get; set; }
        public bool DisplayMotd { get; set; }

        public override string SystemName { get; } = nameof(MotdSystem);
    }
}
