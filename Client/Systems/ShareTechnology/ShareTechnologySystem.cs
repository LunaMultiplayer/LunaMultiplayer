using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologySystem : MessageSystem<ShareTechnologySystem, ShareTechnologyMessageSender, ShareTechnologyMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareTechnologySystem);

        private ShareTechnologyEvents ShareTechnologyEvents { get; } = new ShareTechnologyEvents();
        public bool IgnoreEvents { get; set; }

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

            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            GameEvents.OnTechnologyResearched.Add(ShareTechnologyEvents.TechnologyResearched);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            if (SettingsSystem.ServerSettings.GameMode == GameMode.Sandbox) return;

            GameEvents.OnTechnologyResearched.Remove(ShareTechnologyEvents.TechnologyResearched);
        }

        public void StartIgnoringEvents()
        {
            IgnoreEvents = true;
        }

        public void StopIgnoringEvents()
        {
            IgnoreEvents = false;
        }
    }
}
