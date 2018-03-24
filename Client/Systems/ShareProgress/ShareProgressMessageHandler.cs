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
    public class ShareProgressMessageHandler : SubSystem<ShareProgressSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            switch (msgData.ShareProgressMessageType)
            {
                case ShareProgressMessageType.FundsUpdate:
                    {
                        if (msgData is ShareProgressFundsMsgData data)
                        {
                            FundsUpdate(data);
                        }
                        break;
                    }
                case ShareProgressMessageType.ScienceUpdate:
                    {
                        if (msgData is ShareProgressScienceMsgData data)
                        {
                            ScienceUpdate(data);
                        }
                        break;
                    }
                case ShareProgressMessageType.ReputationUpdate:
                    {
                        if (msgData is ShareProgressReputationMsgData data)
                        {
                            ReputationUpdate(data);
                        }
                        break;
                    }
                case ShareProgressMessageType.TechnologyUpdate:
                    {
                        if (msgData is ShareProgressTechnologyMsgData data)
                        {
                            TechnologyUpdate(data);
                        }
                        break;
                    }
                case ShareProgressMessageType.ContractUpdate:
                    {
                        if (msgData is ShareProgressContractMsgData data)
                        {
                            ContractUpdate(data);
                        }
                        break;
                    }
                case ShareProgressMessageType.MilestoneUpdate:
                    {
                        if (msgData is ShareProgressMilestoneMsgData data)
                        {
                            MilestoneUpdate(data);
                        }
                        break;
                    }
            }
        }

        #region PrivateMethods
        private static void FundsUpdate(ShareProgressFundsMsgData data)
        {
            System.IncomingFundsProcessing = true;
            Funding.Instance.SetFunds(data.Funds, TransactionReasons.None);
            System.IncomingFundsProcessing = false;
            LunaLog.Log("FundsUpdate received - funds changed to: " + data.Funds);
        }

        private static void ScienceUpdate(ShareProgressScienceMsgData data)
        {
            System.IncomingScienceProcessing = true;
            ResearchAndDevelopment.Instance.SetScience(data.Science, TransactionReasons.None);
            System.IncomingScienceProcessing = false;
            LunaLog.Log("ScienceUpdate received - science changed to: " + data.Science);
        }

        private static void ReputationUpdate(ShareProgressReputationMsgData data)
        {
            System.IncomingReputationProcessing = true;
            Reputation.Instance.SetReputation(data.Reputation, TransactionReasons.None);
            System.IncomingReputationProcessing = false;
            LunaLog.Log("ReputationUpdate received - reputation changed to: " + data.Reputation);
        }

        private static void TechnologyUpdate(ShareProgressTechnologyMsgData data)
        {
            System.IncomingTechnologyProcessing = true;
            var nodes = AssetBase.RnDTechTree.GetTreeTechs();
            foreach (var n in nodes)
            {
                if (n.techID == data.TechId)
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(n);
            }

            ResearchAndDevelopment.RefreshTechTreeUI();
            System.IncomingTechnologyProcessing = false;
            LunaLog.Log("TechnologyUpdate received - technology unlocked: " + data.TechId);
        }

        private static void ContractUpdate(ShareProgressContractMsgData data)
        {
            //Don't listen to these events for the time this message is processing.
            System.IncomingContractsProcessing = true;  
            System.IncomingFundsProcessing = true;
            System.IncomingScienceProcessing = true;
            System.IncomingReputationProcessing = true;
            System.SaveBasicProgress(); //Save the current funds, science and reputation for restoring after the contract changes were applied.

            LunaLog.Log("IncomingContractsProcessing=true");

            foreach (var cInfo in data.Contracts)
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
            
            System.RestoreBasicProgress();  //Restore funds, science and reputation in case the contract action changed some of that.
            //Listen to the events again.
            System.IncomingFundsProcessing = false;
            System.IncomingScienceProcessing = false;
            System.IncomingReputationProcessing = false;
            System.IncomingContractsProcessing = false;
            LunaLog.Log("IncomingContractsProcessing=false");
            GameEvents.Contract.onContractsListChanged.Fire();
        }

        private static void MilestoneUpdate(ShareProgressMilestoneMsgData data)
        {
            System.IncomingMilestonesProcessing = true;
            System.IncomingFundsProcessing = true;
            System.IncomingScienceProcessing = true;
            System.IncomingReputationProcessing = true;
            System.SaveBasicProgress(); //Save the current funds, science and reputation for restoring after the milestone changes were applied.

            foreach (var mInfo in data.Milestones)
            {
                var incomingMilestone = ConvertByteArrayToMilestone(mInfo.Data, mInfo.NumBytes, mInfo.Id);
                if (incomingMilestone == null) continue;

                var milestoneIndex = -1;
                for (var i = 0; i < ProgressTracking.Instance.achievementTree.Count; i++)
                {
                    if (ProgressTracking.Instance.achievementTree[i].Id != incomingMilestone.Id) continue;
                    milestoneIndex = i;
                    break;
                }

                if (milestoneIndex != -1)
                {
                    //found the same milestone in the achievementTree
                    if (!ProgressTracking.Instance.achievementTree[milestoneIndex].IsReached && incomingMilestone.IsReached)
                        ProgressTracking.Instance.achievementTree[milestoneIndex].Reach();

                    if (!ProgressTracking.Instance.achievementTree[milestoneIndex].IsComplete && incomingMilestone.IsComplete)
                        ProgressTracking.Instance.achievementTree[milestoneIndex].Complete();

                    LunaLog.Log("Milestone was updated: " + incomingMilestone.Id);
                }
                else
                {
                    //didn't found the same milestone in the achievmentTree
                    ProgressTracking.Instance.achievementTree.AddNode(incomingMilestone);
                    LunaLog.Log("Milestone was added: " + incomingMilestone.Id);
                }
            }

            System.RestoreBasicProgress();  //Restore funds, science and reputation in case the milestone action changed some of that.
            //Listen to the events again.
            System.IncomingFundsProcessing = false;
            System.IncomingScienceProcessing = false;
            System.IncomingReputationProcessing = false;
            System.IncomingMilestonesProcessing = false;
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode and then to a Contract.
        /// If anything goes wrong it will return null.
        /// </summary>
        /// <param name="data">The byte array that represents the configNode</param>
        /// <param name="numBytes">The length of the byte array</param>
        /// <returns></returns>
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
        /// Convert a byte array to a ConfigNode and then to a ProgressNode.
        /// If anything goes wrong it will return null.
        /// </summary>
        /// <param name="data">The byte array that represents the configNode</param>
        /// <param name="numBytes">The length of the byte array</param>
        /// <param name="progressNodeId">The Id of the ProgressNode</param>
        /// <returns></returns>
        private static ProgressNode ConvertByteArrayToMilestone(byte[] data, int numBytes, string progressNodeId)
        {
            ConfigNode node;
            try
            {
                node = ConfigNodeSerializer.Deserialize(data, numBytes);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing milestone configNode: {e}");
                return null;
            }

            if (node == null)
            {
                LunaLog.LogError("[LMP]: Error, the milestone configNode was null.");
                return null;
            }

            ProgressNode milestone;
            try
            {
                var value = node.GetValue("type");
                node.RemoveValues("type");
                var contractType = ContractSystem.GetContractType(value);
                milestone = new ProgressNode(progressNodeId, false);
                milestone.Load(node);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing milestone: {e}");
                return null;
            }

            return milestone;
        }

        /// <summary>
        /// Updates the local contract at given index in the ContractSystem.Instance.Contracts list
        /// with the given incomingContract data.
        /// </summary>
        /// <param name="contractIndex">Index in ContractSystem.Instance.Contracts</param>
        /// <param name="incomingContract">Wanted contract</param>
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

            LunaLog.Log("ContractUpdate received - contract state changed on: " + incomingContract.ContractGuid.ToString() + " - " + incomingContract.Title);
        }

        /// <summary>
        /// Adds a contract to the local ContractSystem.
        /// </summary>
        /// <param name="incomingContract"></param>
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

            LunaLog.Log("New contract added: " + incomingContract.ContractGuid + " - " + incomingContract.Title);
        }
        #endregion
    }
}
