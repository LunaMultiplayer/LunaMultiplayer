using LmpClient.Base;

namespace LmpClient.Systems.Admin
{
    public class AdminSystem : MessageSystem<AdminSystem, AdminMessageSender, AdminMessageHandler>
    {
        public string AdminPassword { get; set; } = string.Empty;

        public override string SystemName { get; } = nameof(AdminSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            AdminPassword = string.Empty;
        }
    }
}
