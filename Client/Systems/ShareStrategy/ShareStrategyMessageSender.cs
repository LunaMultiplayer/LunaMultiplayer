using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using System;
using Strategies;

namespace LunaClient.Systems.ShareStrategy
{
    public class ShareStrategyMessageSender : SubSystem<ShareStrategySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendStrategyMessage(Strategy strategy)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressStrategyMsgData>();
            msgData.Strategy.Title = strategy.Title;
            msgData.Strategy.Factor = strategy.Factor;
            msgData.Strategy.IsActive = strategy.IsActive;

            var configNode = ConvertStrategyToConfigNode(strategy);
            if (configNode == null) return;

            var data = ConfigNodeSerializer.Serialize(configNode);
            var numBytes = data.Length;

            msgData.Strategy.NumBytes = numBytes;
            if (msgData.Strategy.Data.Length < numBytes)
                msgData.Strategy.Data = new byte[numBytes];

            Array.Copy(data, msgData.Strategy.Data, numBytes);

            SendMessage(msgData);
        }

        private static ConfigNode ConvertStrategyToConfigNode(Strategy strategy)
        {
            var configNode = new ConfigNode();
            try
            {
                strategy.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving strategy: {e}");
                return null;
            }

            return configNode;
        }

        
    }
}
