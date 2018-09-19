using LmpClient.Base;

namespace LmpClient.Systems.Motd
{
    public class MotdSystem : MessageSystem<MotdSystem, MotdMessageSender, MotdMessageHandler>
    {
        public override string SystemName { get; } = nameof(MotdSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            MessageSender.SendMotdRequest();
        }
    }
}
