using Contracts;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareAchievements;
using LunaClient.Systems.ShareProgress;
using LunaCommon.Enums;

namespace LunaClient.Systems.ShareContracts
{
    public class ShareContractsSystem : ShareProgressBaseSystem<ShareContractsSystem, ShareContractsMessageSender, ShareContractsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareContractsSystem);

        private ShareContractsEvents ShareContractsEvents { get; } = new ShareContractsEvents();

        private int _defaultContractGenerateIterations;

        //We need to wait for all the achievements to be processed before we process the contracts. Otherwise it will cause bugs.
        protected override bool ShareSystemReady => ContractSystem.Instance != null && ContractSystem.Instance.Contracts.Count != 0 && Funding.Instance != null && ResearchAndDevelopment.Instance != null &&
                                                    Reputation.Instance != null && ShareAchievementsSystem.Singleton.ActionQueueCount == 0;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnEnabled();
            
            _defaultContractGenerateIterations = ContractSystem.generateContractIterations;
            ContractSystem.generateContractIterations = 0;

            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, TryGetContractLock));
            SetupRoutine(new RoutineDefinition(500, RoutineExecution.Update, SetContractGenerationBasedOnLock));

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
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnDisabled();

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

        /// <summary>
        /// Update the ContractSystem generation depending on if the current player has the lock or not.
        /// </summary>
        private void SetContractGenerationBasedOnLock()
        {
            ContractSystem.generateContractIterations = !LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName) ? 0 :
                _defaultContractGenerateIterations;
        }

        /// <summary>
        /// Try to acquire the contract lock
        /// </summary>
        private static void TryGetContractLock()
        {
            if (!LockSystem.LockQuery.ContractLockExists())
            {
                LockSystem.Singleton.AcquireContractLock();
            }
        }
    }
}
