using Contracts;
using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LunaClient.Systems.ShareProgress
{
    /// <summary>
    /// A system for synchronizing progress between the clients
    /// (funds, science, reputation, technology, contracts)
    /// </summary>
    class ShareProgressSystem : MessageSystem<ShareProgressSystem, ShareProgressMessageSender, ShareProgressMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareProgressSystem);

        private ShareProgressEvents ShareProgressEvents { get; } = new ShareProgressEvents();

        public bool IncomingFundsProcessing;
        public bool IncomingScienceProcessing;
        public bool IncomingReputationProcessing;
        public bool IncomingTechnologyProcessing;
        public bool IncomingContractsProcessing;
        public bool IncomingMilestonesProcessing;

        private int _defaultContractGenerateIterations;
        
        private double _savedFunds;
        private float _savedScience;
        private float _savedReputation;

        #region UnityMethods
        protected override void OnEnabled()
        {
            base.OnEnabled();

            IncomingFundsProcessing = false;
            IncomingScienceProcessing = false;
            IncomingReputationProcessing = false;
            IncomingTechnologyProcessing = false;
            IncomingContractsProcessing = false;
            IncomingMilestonesProcessing = false;
            _defaultContractGenerateIterations = ContractSystem.generateContractIterations;
            _savedFunds = 0;
            _savedScience = 0;
            _savedReputation = 0;

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Sandbox)
            {
                SubscribeToBasicEvents();
                SubscribeToRevertEvents();

                if (SettingsSystem.ServerSettings.GameMode == GameMode.Career)
                {
                    TryGetContractLock();
                    SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, TryGetContractLock));

                    SubscribeToContractEvents();
                    SubscribeToMilestoneEvents();
                }
            }
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            if (SettingsSystem.ServerSettings.GameMode != GameMode.Sandbox)
            {
                UnsubscribeFromBasicEvents();
                UnsubscribeFromRevertEvents();

                if (SettingsSystem.ServerSettings.GameMode == GameMode.Career)
                {
                    UnsubscribeFromContractEvents();
                    UnsubscribeFromMilestoneEvents();
                }
            }
        }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Saves the current funds, science and reputation in memory.
        /// So they can be later applied again with RestoreBasicProgress.
        /// </summary>
        public void SaveBasicProgress()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Sandbox)
            {
                _savedScience = ResearchAndDevelopment.Instance.Science;

                if (SettingsSystem.ServerSettings.GameMode == GameMode.Career)
                {
                    _savedFunds = Funding.Instance.Funds;
                    _savedReputation = Reputation.Instance.reputation;
                }
            }
        }

        /// <summary>
        /// Restores the funds, science and repuation that was saved before.
        /// </summary>
        public void RestoreBasicProgress()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Sandbox)
            {
                ResearchAndDevelopment.Instance.SetScience(_savedScience, TransactionReasons.None);

                if (SettingsSystem.ServerSettings.GameMode == GameMode.Career)
                {
                    Funding.Instance.SetFunds(_savedFunds, TransactionReasons.None);
                    Reputation.Instance.SetReputation(_savedReputation, TransactionReasons.None);
                }
            }
        }
        #endregion

        #region PrivateMethods
        private void SubscribeToBasicEvents()
        {
            GameEvents.OnFundsChanged.Add(ShareProgressEvents.FundsChanged);
            GameEvents.OnReputationChanged.Add(ShareProgressEvents.ReputationChanged);
            GameEvents.OnScienceChanged.Add(ShareProgressEvents.ScienceChanged);
            GameEvents.OnTechnologyResearched.Add(ShareProgressEvents.TechnologyResearched);
        }

        private void UnsubscribeFromBasicEvents()
        {
            GameEvents.OnFundsChanged.Remove(ShareProgressEvents.FundsChanged);
            GameEvents.OnReputationChanged.Remove(ShareProgressEvents.ReputationChanged);
            GameEvents.OnScienceChanged.Remove(ShareProgressEvents.ScienceChanged);
            GameEvents.OnTechnologyResearched.Remove(ShareProgressEvents.TechnologyResearched);
        }

        private void SubscribeToContractEvents()
        {
            GameEvents.Contract.onAccepted.Add(ShareProgressEvents.ContractAccepted);
            GameEvents.Contract.onCancelled.Add(ShareProgressEvents.ContractCancelled);
            GameEvents.Contract.onCompleted.Add(ShareProgressEvents.ContractCompleted);
            GameEvents.Contract.onContractsListChanged.Add(ShareProgressEvents.ContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Add(ShareProgressEvents.ContractsLoaded);
            GameEvents.Contract.onDeclined.Add(ShareProgressEvents.ContractDeclined);
            GameEvents.Contract.onFailed.Add(ShareProgressEvents.ContractFailed);
            GameEvents.Contract.onFinished.Add(ShareProgressEvents.ContractFinished);
            GameEvents.Contract.onOffered.Add(ShareProgressEvents.ContractOffered);
            GameEvents.Contract.onParameterChange.Add(ShareProgressEvents.ContractParameterChanged);
            GameEvents.Contract.onRead.Add(ShareProgressEvents.ContractRead);
            GameEvents.Contract.onSeen.Add(ShareProgressEvents.ContractSeen);
        }

        private void UnsubscribeFromContractEvents()
        {
            GameEvents.Contract.onAccepted.Remove(ShareProgressEvents.ContractAccepted);
            GameEvents.Contract.onCancelled.Remove(ShareProgressEvents.ContractCancelled);
            GameEvents.Contract.onCompleted.Remove(ShareProgressEvents.ContractCompleted);
            GameEvents.Contract.onContractsListChanged.Remove(ShareProgressEvents.ContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Remove(ShareProgressEvents.ContractsLoaded);
            GameEvents.Contract.onDeclined.Remove(ShareProgressEvents.ContractDeclined);
            GameEvents.Contract.onFailed.Remove(ShareProgressEvents.ContractFailed);
            GameEvents.Contract.onFinished.Remove(ShareProgressEvents.ContractFinished);
            GameEvents.Contract.onOffered.Remove(ShareProgressEvents.ContractOffered);
            GameEvents.Contract.onParameterChange.Remove(ShareProgressEvents.ContractParameterChanged);
            GameEvents.Contract.onRead.Remove(ShareProgressEvents.ContractRead);
            GameEvents.Contract.onSeen.Remove(ShareProgressEvents.ContractSeen);
        }

        private void SubscribeToMilestoneEvents()
        {
            GameEvents.OnProgressReached.Add(ShareProgressEvents.MilestoneReached);
            GameEvents.OnProgressComplete.Add(ShareProgressEvents.MilestoneCompleted);
            GameEvents.OnProgressAchieved.Add(ShareProgressEvents.MilestoneAchieved);
        }

        private void UnsubscribeFromMilestoneEvents()
        {
            GameEvents.OnProgressReached.Remove(ShareProgressEvents.MilestoneReached);
            GameEvents.OnProgressComplete.Remove(ShareProgressEvents.MilestoneCompleted);
            GameEvents.OnProgressAchieved.Remove(ShareProgressEvents.MilestoneAchieved);
        }

        private void SubscribeToRevertEvents()
        {
            RevertEvent.onReturnToEditor.Add(ShareProgressEvents.RevertToEditor);
            RevertEvent.onRevertToLaunch.Add(ShareProgressEvents.RevertToLaunch);
            RevertEvent.onRevertToPrelaunch.Add(ShareProgressEvents.RevertToPreLaunch);
        }

        private void UnsubscribeFromRevertEvents()
        {
            RevertEvent.onReturnToEditor.Remove(ShareProgressEvents.RevertToEditor);
            RevertEvent.onRevertToLaunch.Remove(ShareProgressEvents.RevertToLaunch);
            RevertEvent.onRevertToPrelaunch.Remove(ShareProgressEvents.RevertToPreLaunch);
        }

        private void TryGetContractLock()
        {
            if (!LockSystem.LockQuery.ContractLockExists())
            {
                LockSystem.Singleton.AcquireContractLock();
            }

            //Update the ContractSystem generation depending on if the current player has the lock or not.
            if (!LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
            {
                ContractSystem.generateContractIterations = 0;
                LunaLog.Log("You have no ContractLock and are not allowed to generate contracts.");
            }
            else
            {
                ContractSystem.generateContractIterations = _defaultContractGenerateIterations;
                LunaLog.Log("You have the ContractLock and you will generate contracts.");
            }
        }
        #endregion
    }
}
