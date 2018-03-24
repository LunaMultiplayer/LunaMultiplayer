using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareScience
{
    public class ShareScienceSystem : MessageSystem<ShareScienceSystem, ShareScienceMessageSender, ShareScienceMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareScienceSystem);

        private ShareScienceEvents ShareScienceEvents { get; } = new ShareScienceEvents();
        public bool IgnoreEvents { get; set; }
        private float _lastScience;

        protected override void NetworkEventHandler(ClientState data)
        {
            if (data <= ClientState.Disconnected)
            {
                Enabled = false;
            }

            if (data == ClientState.Running && SettingsSystem.ServerSettings.ShareProgress &&
                (SettingsSystem.ServerSettings.GameMode == GameMode.Science ||
                 SettingsSystem.ServerSettings.GameMode == GameMode.Career))
            {
                Enabled = true;
            }
        }

        protected override void OnEnabled()
        {
            base.OnEnabled();

            IgnoreEvents = false;
            _lastScience = 0;

            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            GameEvents.OnScienceChanged.Add(ShareScienceEvents.ScienceChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            GameEvents.OnScienceChanged.Remove(ShareScienceEvents.ScienceChanged);
        }

        public void StartIgnoringEvents()
        {
            if (ResearchAndDevelopment.Instance != null)
                _lastScience = ResearchAndDevelopment.Instance.Science;

            IgnoreEvents = true;
        }

        public void StopIgnoringEvents(bool restoreOldValue = false)
        {
            if (restoreOldValue && ResearchAndDevelopment.Instance != null)
                ResearchAndDevelopment.Instance.SetScience(_lastScience, TransactionReasons.None);

            IgnoreEvents = false;
        }
    }
}
