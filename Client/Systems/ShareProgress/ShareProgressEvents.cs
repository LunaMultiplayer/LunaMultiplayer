using Contracts;
using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaClient.Systems.ShareProgress
{
    class ShareProgressEvents : SubSystem<ShareProgressSystem>
    {
        #region EventHandling
        public void FundsChanged(double value, TransactionReasons reason)
        {
            if (System.IncomingFundsProcessing)
                return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressFundsMsgData>();
            msgData.Funds = value;
            msgData.Reason = reason.ToString();
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log("Funds changed to: " + value + " with reason: " + reason.ToString());
        }

        public void ScienceChanged(float value, TransactionReasons reason)
        {
            if (System.IncomingScienceProcessing)
                return;
            
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressScienceMsgData>();
            msgData.Science = value;
            msgData.Reason = reason.ToString();
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log("Science changed to: " + value + " with reason: " + reason.ToString());
        }

        public void ReputationChanged(float value, TransactionReasons reason)
        {
            if (System.IncomingReputationProcessing)
                return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressReputationMsgData>();
            msgData.Reputation = value;
            msgData.Reason = reason.ToString();
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log("Reputation changed to: " + value + " with reason: " + reason.ToString());
        }

        public void TechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (System.IncomingTechnologyProcessing)
                return;

            if (data.target == RDTech.OperationResult.Successful)
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressTechnologyMsgData>();
                msgData.TechID = data.host.techID;
                LunaLog.Log("Technology unlocked: " + msgData.TechID);
                System.MessageSender.SendMessage(msgData);
            }
        }

        public void ContractAccepted(Contract contract)
        {
            if (System.IncomingContractsProcessing)
                return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract accepted: " + contract.ContractGuid);
        }

        public void ContractCancelled(Contract contract)
        {
            if (System.IncomingContractsProcessing)
                return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract cancelled: " + contract.ContractGuid);
        }

        public void ContractCompleted(Contract contract)
        {
            if (System.IncomingContractsProcessing)
                return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract completed: " + contract.ContractGuid);
        }

        public void ContractsListChanged()
        {
            LunaLog.Log("Contract list changed.");
        }

        public void ContractsLoaded()
        {
            LunaLog.Log("Contracts loaded.");
        }

        public void ContractDeclined(Contract contract)
        {
            if (System.IncomingContractsProcessing)
                return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract declined: " + contract.ContractGuid);
        }

        public void ContractFailed(Contract contract)
        {
            if (System.IncomingContractsProcessing)
                return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract failed: " + contract.ContractGuid);
        }

        public void ContractFinished(Contract contract)
        {
            /*
            Doesn't need to be synchronized because there is no ContractFinished state.
            Also the contract will be finished on the contract complete / failed / cancelled / ...
            */
        }

        public void ContractOffered(Contract contract)
        {
            LunaLog.Log("Contract offered: " + contract.ContractGuid + " - " + contract.Title);

            //This should be only called on the client with the contract lock, because he has the generationCount != 0.
            if (LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
            {
                SendContractUpdate(contract);

                //Also store the current contracts on the server so new players will see this contract too.
                ScenarioSystem.Singleton.SendScenarioModules();
            }
        }

        public void ContractParameterChanged(Contract contract, ContractParameter contractParameter)
        {
            SendContractUpdate(contract);
            LunaLog.Log("Contract parameter changed on:" + contract.ContractGuid.ToString());
        }

        public void ContractRead(Contract contract)
        {
            LunaLog.Log("Contract read:" + contract.ContractGuid.ToString());
        }

        public void ContractSeen(Contract contract)
        {
            LunaLog.Log("Contract seen:" + contract.ContractGuid.ToString());
        }
        
        public void MilestoneReached(ProgressNode progressNode)
        {
            if (System.IncomingMilestonesProcessing)
                return;

            SendMilestoneUpdate(progressNode);
            LunaLog.Log("Milestone reached:" + progressNode.Id);
        }
        
        public void MilestoneCompleted(ProgressNode progressNode)
        {
            if (System.IncomingMilestonesProcessing)
                return;

            SendMilestoneUpdate(progressNode);
            LunaLog.Log("Milestone completed:" + progressNode.Id);
        }
        
        public void MilestoneAchieved(ProgressNode progressNode)
        {
            //This event is triggered to often (always if some speed or distance record changes).
            //LunaLog.Log("Progress achieved:" + progressNode.Id);
        }

        public void RevertToEditor(EditorFacility editorFacility)
        {
            //Looks like the funds, science and reputation values are reverted after this event
            //otherwise maybe it could be as simple as sending the current funds, science and reputation back to all other clients.
            //if thats not the case we need some manual log of the transactions and revert from there...

            LunaLog.Log("Reverted progress.");
        }

        public void RevertToLaunch()
        {
            //Looks like the funds, science and reputation values are reverted after this event
            //otherwise maybe it could be as simple as sending the current funds, science and reputation back to all other clients.
            //if thats not the case we need some manual log of the transactions and revert from there...

            LunaLog.Log("Reverted progress.");
        }

        public void RevertToPreLaunch(EditorFacility editorFacility)
        {
            //Looks like the funds, science and reputation values are reverted after this event
            //otherwise maybe it could be as simple as sending the current funds, science and reputation back to all other clients.
            //if thats not the case we need some manual log of the transactions and revert from there...

            LunaLog.Log("Reverted progress.");
        }
        #endregion

        #region PrivateMethods
        private void SendContractUpdate(Contract[] contracts)
        {
            //Convert the Contract's to ContractInfo's.
            var contractInfos = new List<ContractInfo>();
            foreach (var contract in contracts)
            {
                var configNode = ConvertContractToConfigNode(contract);
                if (configNode == null)
                {
                    break;
                }

                var data = ConfigNodeSerializer.Serialize(configNode);
                var numBytes = data.Length;

                contractInfos.Add(new ContractInfo()
                {
                    ContractGuid = contract.ContractGuid,
                    Data = data,
                    NumBytes = numBytes
                });
            }

            //Build the packet and send it.
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressContractMsgData>();
            msgData.Contracts = contractInfos.ToArray();
            msgData.ContractCount = msgData.Contracts.Length;
            System.MessageSender.SendMessage(msgData);
        }

        private void SendContractUpdate(Contract contract)
        {
            SendContractUpdate(new Contract[] { contract });
        }

        private ConfigNode ConvertContractToConfigNode(Contract contract)
        {
            var configNode = new ConfigNode();
            try
            {
                contract.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving contract: {e}");
                return null;
            }

            return configNode;
        }

        private void SendMilestoneUpdate(ProgressNode[] milestones)
        {
            //Convert the Contract's to ContractInfo's.
            var milestoneInfos = new List<MilestoneInfo>();
            foreach (var milestone in milestones)
            {
                var configNode = ConvertMilestoneToConfigNode(milestone);
                if (configNode == null)
                {
                    break;
                }

                var data = ConfigNodeSerializer.Serialize(configNode);
                var numBytes = data.Length;

                milestoneInfos.Add(new MilestoneInfo()
                {
                    Id = milestone.Id,
                    Data = data,
                    NumBytes = numBytes
                });
            }

            //Build the packet and send it.
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressMilestoneMsgData>();
            msgData.Milestones = milestoneInfos.ToArray();
            msgData.MilestoneCount = msgData.Milestones.Length;
            System.MessageSender.SendMessage(msgData);
        }

        private void SendMilestoneUpdate(ProgressNode milestone)
        {
            SendMilestoneUpdate(new ProgressNode[] { milestone });
        }

        private ConfigNode ConvertMilestoneToConfigNode(ProgressNode milestone)
        {
            var configNode = new ConfigNode();
            try
            {
                milestone.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving contract: {e}");
                return null;
            }

            return configNode;
        }
        #endregion
    }
}
