using System;
using System.Collections.Generic;
using Contracts;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Utilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.ShareContracts
{
    public class ShareContractsMessageSender : SubSystem<ShareContractsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }
        
        public void SendContractMessage(Contract[] contracts)
        {
            //Convert the Contract's to ContractInfo's.
            var contractInfos = new List<ContractInfo>();
            foreach (var contract in contracts)
            {
                var configNode = ConvertContractToConfigNode(contract);
                if (configNode == null) break;

                var data = ConfigNodeSerializer.Serialize(configNode);
                var numBytes = data.Length;

                contractInfos.Add(new ContractInfo
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

        public void SendContractMessage(Contract contract)
        {
            SendContractMessage(new[] { contract });
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
    }
}
