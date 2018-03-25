using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
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

        protected override bool ActionDependencyReady()
        {
            return (ResearchAndDevelopment.Instance != null);
        }

        public override void SaveState()
        {
            _lastScience = ResearchAndDevelopment.Instance.Science;
        }

        public override void RestoreState()
        {
            ResearchAndDevelopment.Instance.SetScience(_lastScience, TransactionReasons.None);
        }
    }
}
