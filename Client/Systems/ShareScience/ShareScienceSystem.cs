using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareScience
{
    public class ShareScienceSystem : ShareProgressBaseSystem<ShareScienceSystem, ShareScienceMessageSender, ShareScienceMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareScienceSystem);

        private ShareScienceEvents ShareScienceEvents { get; } = new ShareScienceEvents();

        private float _lastScience;

        protected override bool ShareSystemReady => ResearchAndDevelopment.Instance != null;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnEnabled();
            GameEvents.OnScienceChanged.Add(ShareScienceEvents.ScienceChanged);
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            base.OnDisabled();
            GameEvents.OnScienceChanged.Remove(ShareScienceEvents.ScienceChanged);
            _lastScience = 0;
        }

        public override void SaveState()
        {
            base.SaveState();
            _lastScience = ResearchAndDevelopment.Instance.Science;
        }

        public override void RestoreState()
        {
            base.RestoreState();
            ResearchAndDevelopment.Instance.SetScience(_lastScience, TransactionReasons.None);
        }
    }
}
