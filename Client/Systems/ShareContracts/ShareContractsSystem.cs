using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareContracts
{
    public class ShareContractsSystem : MessageSystem<ShareContractsSystem, ShareContractsMessageSender, ShareContractsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareContractsSystem);

        private ShareContractsEvents ShareContractsEvents { get; } = new ShareContractsEvents();
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

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            GameEvents.Contract.onAccepted.Add(ShareContractsEvents.ContractAccepted);
            GameEvents.Contract.onCancelled.Add(ShareContractsEvents.ContractCancelled);
            GameEvents.Contract.onCompleted.Add(ShareContractsEvents.ContractCompleted);
            GameEvents.Contract.onContractsListChanged.Add(ShareContractsEvents.ContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Add(ShareContractsEvents.ContractsLoaded);
            GameEvents.Contract.onDeclined.Add(ShareContractsEvents.ContractDeclined);
            GameEvents.Contract.onFailed.Add(ShareContractsEvents.ContractFailed);
            GameEvents.Contract.onFinished.Add(ShareContractsEvents.ContractFinished);
            GameEvents.Contract.onOffered.Add(ShareContractsEvents.ContractOffered);
            GameEvents.Contract.onParameterChange.Add(ShareContractsEvents.ContractParameterChanged);
            GameEvents.Contract.onRead.Add(ShareContractsEvents.ContractRead);
            GameEvents.Contract.onSeen.Add(ShareContractsEvents.ContractSeen);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            GameEvents.Contract.onAccepted.Remove(ShareContractsEvents.ContractAccepted);
            GameEvents.Contract.onCancelled.Remove(ShareContractsEvents.ContractCancelled);
            GameEvents.Contract.onCompleted.Remove(ShareContractsEvents.ContractCompleted);
            GameEvents.Contract.onContractsListChanged.Remove(ShareContractsEvents.ContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Remove(ShareContractsEvents.ContractsLoaded);
            GameEvents.Contract.onDeclined.Remove(ShareContractsEvents.ContractDeclined);
            GameEvents.Contract.onFailed.Remove(ShareContractsEvents.ContractFailed);
            GameEvents.Contract.onFinished.Remove(ShareContractsEvents.ContractFinished);
            GameEvents.Contract.onOffered.Remove(ShareContractsEvents.ContractOffered);
            GameEvents.Contract.onParameterChange.Remove(ShareContractsEvents.ContractParameterChanged);
            GameEvents.Contract.onRead.Remove(ShareContractsEvents.ContractRead);
            GameEvents.Contract.onSeen.Remove(ShareContractsEvents.ContractSeen);
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
