using LunaClient.Base;
using LunaCommon.Enums;

namespace LunaClient.Systems.Admin
{
    public class AdminSystem : MessageSystem<AdminSystem, AdminMessageSender, AdminMessageHandler>
    {
        public string AdminPassword { get; set; } = string.Empty;
        public AdminResponse? LastCommandResponse { get; set; }

        public override string SystemName { get; } = nameof(AdminSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            AdminPassword = string.Empty;
            LastCommandResponse = null;
        }
    }
}
