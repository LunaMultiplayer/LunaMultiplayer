using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;

namespace LunaClient.Systems.ShareContracts
{
    public class ShareContractsEvents : SubSystem<ShareContractsSystem>
    {
        #region EventHandlers
        public void ContractAccepted(Contract contract)
        {
            if (System.IgnoreEvents) return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract accepted: " + contract.ContractGuid);
        }

        public void ContractCancelled(Contract contract)
        {
            if (System.IgnoreEvents) return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract cancelled: " + contract.ContractGuid);
        }

        public void ContractCompleted(Contract contract)
        {
            if (System.IgnoreEvents) return;

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
            if (System.IgnoreEvents) return;

            SendContractUpdate(contract);
            LunaLog.Log("Contract declined: " + contract.ContractGuid);
        }

        public void ContractFailed(Contract contract)
        {
            if (System.IgnoreEvents) return;

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

            if (!LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName)) return;

            //This should be only called on the client with the contract lock, because he has the generationCount != 0.
            SendContractUpdate(contract);

            //Also store the current contracts on the server so new players will see this contract too.
            ScenarioSystem.Singleton.SendScenarioModules();
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
        #endregion

        #region PrivateMethods
        private static void SendContractUpdate(Contract[] contracts)
        {
            //Convert the Contract's to ContractInfo's.
            var contractInfos = new List<ContractInfo>();
            foreach (var contract in contracts)
            {
                var configNode = ConvertContractToConfigNode(contract);
                if (configNode == null) break;

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
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressContractsMsgData>();
            msgData.Contracts = contractInfos.ToArray();
            msgData.ContractCount = msgData.Contracts.Length;
            System.MessageSender.SendMessage(msgData);
        }

        private static void SendContractUpdate(Contract contract)
        {
            SendContractUpdate(new Contract[] { contract });
        }

        private static ConfigNode ConvertContractToConfigNode(Contract contract)
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
        #endregion
    }
}
