using System;
using System.Collections.Concurrent;
using Contracts;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.ShareCareer;
using LmpClient.Systems.ShareExperimentalParts;
using LmpClient.Systems.ShareFunds;
using LmpClient.Systems.ShareReputation;
using LmpClient.Systems.ShareScience;
using LmpClient.Utilities;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.ShareContracts
{
    public class ShareContractsMessageHandler : SubSystem<ShareContractsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ContractsUpdate) return;

            if (msgData is ShareProgressContractsMsgData data)
            {
                var contractInfos = CopyContracts(data.Contracts); //create a deep copy of the contracts array so it will not change in the future.

                LunaLog.Log("Queue ContractsUpdate.");
                ShareCareerSystem.Singleton.QueueAction(() =>
                {
                    ContractUpdate(contractInfos);
                });
            }
        }

        private static ContractInfo[] CopyContracts(ContractInfo[] contracts)
        {
            var newContracts = new ContractInfo[contracts.Length];
            for (var i = 0; i < contracts.Length; i++)
            {
                newContracts[i] = new ContractInfo(contracts[i]);
            }

            return newContracts;
        }

        private static void ContractUpdate(ContractInfo[] contractInfos)
        {
            //Don't listen to these events for the time this message is processing.
            System.StartIgnoringEvents();
            ShareFundsSystem.Singleton.StartIgnoringEvents();
            ShareScienceSystem.Singleton.StartIgnoringEvents();
            ShareReputationSystem.Singleton.StartIgnoringEvents();
            ShareExperimentalPartsSystem.Singleton.StartIgnoringEvents();

            foreach (var cInfo in contractInfos)
            {
                var incomingContract = ConvertByteArrayToContract(cInfo.Data, cInfo.NumBytes);
                if (incomingContract == null) continue;

                var contractIndex = ContractSystem.Instance.Contracts.FindIndex(c => c.ContractGuid == cInfo.ContractGuid);

                if (contractIndex != -1)
                {
                    //found the contract in the local ContractSystem
                    UpdateContract(contractIndex, incomingContract);
                }
                else
                {
                    //There is no matching contract in the local ContractSystem
                    AddContract(incomingContract);
                }
            }

            //Listen to the events again.
            //Restore funds, science and reputation in case the contract action changed some of that.
            ShareFundsSystem.Singleton.StopIgnoringEvents(true);
            ShareScienceSystem.Singleton.StopIgnoringEvents(true);
            ShareReputationSystem.Singleton.StopIgnoringEvents(true);
            ShareExperimentalPartsSystem.Singleton.StopIgnoringEvents();
            GameEvents.Contract.onContractsListChanged.Fire();
            System.StopIgnoringEvents();
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode and then to a Contract.
        /// If anything goes wrong it will return null.
        /// </summary>
        private static Contract ConvertByteArrayToContract(byte[] data, int numBytes)
        {
            ConfigNode node;
            try
            {
                node = ConfigNodeSerializer.Deserialize(data, numBytes);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing contract configNode: {e}");
                return null;
            }

            if (node == null)
            {
                LunaLog.LogError("[LMP]: Error, the contract configNode was null.");
                return null;
            }

            Contract contract;
            try
            {
                var value = node.GetValue("type");
                node.RemoveValues("type");
                var contractType = ContractSystem.GetContractType(value);
                contract = Contract.Load((Contract)Activator.CreateInstance(contractType), node);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing contract: {e}");
                return null;
            }

            return contract;
        }

        /// <summary>
        /// Updates the local contract at given index in the ContractSystem.Instance.Contracts list
        /// with the given incomingContract data.
        /// </summary>
        private static void UpdateContract(int contractIndex, Contract incomingContract)
        {
            if (ContractSystem.Instance.Contracts[contractIndex].ContractState != incomingContract.ContractState)
            {
                //Do the same action on the contract that the incoming contract has already done.
                switch (incomingContract.ContractState)
                {
                    case Contract.State.Active:
                        ContractSystem.Instance.Contracts[contractIndex].Accept();
                        break;
                    case Contract.State.Cancelled:
                        ContractSystem.Instance.Contracts[contractIndex].Cancel();
                        break;
                    case Contract.State.Completed:
                        ContractSystem.Instance.Contracts[contractIndex].Complete();
                        break;
                    case Contract.State.Declined:
                        ContractSystem.Instance.Contracts[contractIndex].Decline();
                        break;
                    case Contract.State.Failed:
                        ContractSystem.Instance.Contracts[contractIndex].Fail();
                        break;
                    case Contract.State.Offered:
                        ContractSystem.Instance.Contracts[contractIndex].Offer();
                        break;
                    case Contract.State.Withdrawn:
                        ContractSystem.Instance.Contracts[contractIndex].Withdraw();
                        break;
                }
            }
            else
            {
                //The incoming contract has the same state as the current one (so it doesn't have changed).

                //Maybe update the parameters and trigger some parameter changed event or something simelar.

                //Or replace the complete contract and hope everything goes fine:
                ContractSystem.Instance.Contracts[contractIndex].Unregister();
                ContractSystem.Instance.Contracts[contractIndex] = incomingContract;
                if (ContractSystem.Instance.Contracts[contractIndex].ContractState == Contract.State.Active)
                {
                    ContractSystem.Instance.Contracts[contractIndex].Register();
                }
            }

            LunaLog.Log($"Contract update received - contract state changed on: {incomingContract.ContractGuid} - {incomingContract.Title}");
        }

        /// <summary>
        /// Adds a contract to the local ContractSystem.
        /// </summary>
        private static void AddContract(Contract incomingContract)
        {
            if (!incomingContract.IsFinished())
            {
                ContractSystem.Instance.Contracts.Add(incomingContract);
                var contractIndex = ContractSystem.Instance.Contracts.FindIndex(c => c.ContractGuid == incomingContract.ContractGuid);

                //Trigger the contract events manually because the incoming contract object has already the state that it should have.
                ContractSystem.Instance.Contracts[contractIndex].OnStateChange.Fire(incomingContract.ContractState);
                switch (ContractSystem.Instance.Contracts[contractIndex].ContractState)
                {
                    case Contract.State.Active:
                        ContractSystem.Instance.Contracts[contractIndex].Register();
                        GameEvents.Contract.onAccepted.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                    case Contract.State.Cancelled:
                        GameEvents.Contract.onCancelled.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        GameEvents.Contract.onFailed.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        GameEvents.Contract.onFinished.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                    case Contract.State.Completed:
                        GameEvents.Contract.onCompleted.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        GameEvents.Contract.onFinished.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                    case Contract.State.Declined:
                        GameEvents.Contract.onDeclined.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                    case Contract.State.Failed:
                        GameEvents.Contract.onFailed.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        GameEvents.Contract.onFinished.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                    case Contract.State.Offered:
                        GameEvents.Contract.onOffered.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                    case Contract.State.Withdrawn:
                        GameEvents.Contract.onFinished.Fire(ContractSystem.Instance.Contracts[contractIndex]);
                        break;
                }
            }
            else
            {
                incomingContract.Unregister();
                if (incomingContract.ContractState == Contract.State.Completed ||
                    incomingContract.ContractState == Contract.State.DeadlineExpired ||
                    incomingContract.ContractState == Contract.State.Failed ||
                    incomingContract.ContractState == Contract.State.Cancelled)
                {
                    ContractSystem.Instance.ContractsFinished.Add(incomingContract);
                }
            }

            LunaLog.Log($"New contract received: {incomingContract.ContractGuid} - {incomingContract.Title}");
        }
    }
}
