using Contracts;
using LmpClient.Events;
using LmpClient.Systems.Lock;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;

namespace LmpClient.Systems.ShareContracts
{
    public class ShareContractsSystem : ShareProgressBaseSystem<ShareContractsSystem, ShareContractsMessageSender, ShareContractsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareContractsSystem);

        private ShareContractsEvents ShareContractsEvents { get; } = new ShareContractsEvents();

        public int DefaultContractGenerateIterations;

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;

            ContractSystem.generateContractIterations = 0;

            LockEvent.onLockAcquireUnityThread.Add(ShareContractsEvents.LockAcquire);
            LockEvent.onLockReleaseUnityThread.Add(ShareContractsEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareContractsEvents.LevelLoaded);

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

            ContractSystem.generateContractIterations = DefaultContractGenerateIterations;

            LockEvent.onLockAcquireUnityThread.Remove(ShareContractsEvents.LockAcquire);
            LockEvent.onLockReleaseUnityThread.Remove(ShareContractsEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareContractsEvents.LevelLoaded);

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
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
        /// Try to acquire the contract lock
        /// </summary>
        public void TryGetContractLock()
        {
            if (!LockSystem.LockQuery.ContractLockExists())
            {
                LockSystem.Singleton.AcquireContractLock();
            }
        }
    }
}
