using Contracts;
using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaClient.Systems.ShareProgress
{
    class ShareProgressMessageHandler : SubSystem<ShareProgressSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            switch (msgData.ShareProgressMessageType)
            {
                case ShareProgressMessageType.FundsUpdate:
                    {
                        this.FundsUpdate((ShareProgressFundsMsgData)msgData);
                        break;
                    }
                case ShareProgressMessageType.ScienceUpdate:
                    {
                        this.ScienceUpdate((ShareProgressScienceMsgData)msgData);
                        break;
                    }
                case ShareProgressMessageType.ReputationUpdate:
                    {
                        this.ReputationUpdate((ShareProgressReputationMsgData)msgData);
                        break;
                    }
                case ShareProgressMessageType.TechnologyUpdate:
                    {
                        this.TechnologyUpdate((ShareProgressTechnologyMsgData)msgData);
                        break;
                    }
                case ShareProgressMessageType.ContractUpdate:
                    {
                        this.ContractUpdate((ShareProgressContractMsgData)msgData);
                        break;
                    }
            }
        }

        #region PrivateMethods
        private void FundsUpdate(ShareProgressFundsMsgData data)
        {
            System.IncomingFundsProcessing = true;
            Funding.Instance.SetFunds(data.Funds, TransactionReasons.None);
            System.IncomingFundsProcessing = false;
            LunaLog.Log("FundsUpdate received - funds changed to: " + data.Funds);
        }

        private void ScienceUpdate(ShareProgressScienceMsgData data)
        {
            System.IncomingScienceProcessing = true;
            ResearchAndDevelopment.Instance.SetScience(data.Science, TransactionReasons.None);
            System.IncomingScienceProcessing = false;
            LunaLog.Log("ScienceUpdate received - science changed to: " + data.Science);
        }

        private void ReputationUpdate(ShareProgressReputationMsgData data)
        {
            System.IncomingReputationProcessing = true;
            Reputation.Instance.SetReputation(data.Reputation, TransactionReasons.None);
            System.IncomingReputationProcessing = false;
            LunaLog.Log("ReputationUpdate received - reputation changed to: " + data.Reputation);
        }

        private void TechnologyUpdate(ShareProgressTechnologyMsgData data)
        {
            System.IncomingTechnologyProcessing = true;
            ProtoTechNode[] nodes = AssetBase.RnDTechTree.GetTreeTechs();
            foreach (ProtoTechNode n in nodes)
            {
                if (n.techID == data.TechID)
                {
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(n);
                }
            }

            ResearchAndDevelopment.RefreshTechTreeUI();
            System.IncomingTechnologyProcessing = false;
            LunaLog.Log("TechnologyUpdate received - technology unlocked: " + data.TechID);
        }

        private void ContractUpdate(ShareProgressContractMsgData data)
        {
            //Don't listen to these events for the time this message is processing.
            System.IncomingContractsProcessing = true;  
            System.IncomingFundsProcessing = true;
            System.IncomingScienceProcessing = true;
            System.IncomingReputationProcessing = true;
            System.SaveBasicProgress(); //Save the current funds, science and reputation for restoring after the contract changes were applied.

            LunaLog.Log("IncomingContractsProcessing=true");

            foreach (ContractInfo cInfo in data.Contracts)
            {
                Contract incomingContract = this.ConvertByteArrayToContract(cInfo.Data, cInfo.NumBytes);
                if (incomingContract == null)
                    break;

                int contractIndex = ContractSystem.Instance.Contracts.FindIndex(c => c.ContractGuid == cInfo.ContractGuid);

                if (contractIndex != -1)
                {
                    //found the contract in the local ContractSystem
                    this.UpdateContract(contractIndex, incomingContract);
                }
                else
                {
                    //There is no matching contract in the local ContractSystem
                    this.AddContract(incomingContract);
                }
            }
            
            System.RestoreBasicProgress();  //Restore funds, science and reputation in case the contract action changed some of that.
            //Listen to the events again.
            System.IncomingFundsProcessing = false;
            System.IncomingScienceProcessing = false;
            System.IncomingReputationProcessing = false;
            System.IncomingContractsProcessing = false;
            LunaLog.Log("IncomingContractsProcessing=false");
            GameEvents.Contract.onContractsListChanged.Fire();
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode and the to a Contract.
        /// If anything goes wrong it will return null.
        /// </summary>
        /// <param name="data">The byte array that represents the configNode</param>
        /// <param name="numBytes">The length of the byte array</param>
        /// <returns></returns>
        private Contract ConvertByteArrayToContract(byte[] data, int numBytes)
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
                string value = node.GetValue("type");
                node.RemoveValues("type");
                Type contractType = ContractSystem.GetContractType(value);
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
        /// <param name="contractIndex">Index in ContractSystem.Instance.Contracts</param>
        /// <param name="incomingContract">Wanted contract</param>
        private void UpdateContract(int contractIndex, Contract incomingContract)
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

            LunaLog.Log("ContractUpdate received - contract state changed on: " + incomingContract.ContractGuid.ToString() + " - " + incomingContract.Title);
        }

        /// <summary>
        /// Adds a contract to the local ContractSystem.
        /// </summary>
        /// <param name="incomingContract"></param>
        private void AddContract(Contract incomingContract)
        {
            if (!incomingContract.IsFinished())
            {
                ContractSystem.Instance.Contracts.Add(incomingContract);
                int contractIndex = ContractSystem.Instance.Contracts.FindIndex(c => c.ContractGuid == incomingContract.ContractGuid);
                
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
                if (incomingContract.ContractState == Contract.State.Completed || incomingContract.ContractState == Contract.State.DeadlineExpired || incomingContract.ContractState == Contract.State.Failed || incomingContract.ContractState == Contract.State.Cancelled)
                {
                    ContractSystem.Instance.ContractsFinished.Add(incomingContract);
                }
            }

            LunaLog.Log("New contract added: " + incomingContract.ContractGuid + " - " + incomingContract.Title);
        }
        #endregion
    }
}
