using LmpClient.Base;

namespace LmpClient.Systems.PlayerConnection
{
    public class PlayerConnectionSystem : MessageSystem<PlayerConnectionSystem, PlayerConnectionMessageSender, PlayerConnectionMessageHandler>
    {
        public override string SystemName { get; } = nameof(PlayerConnectionSystem);
    }
}
