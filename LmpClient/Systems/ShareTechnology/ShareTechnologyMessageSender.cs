using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.ShareTechnology
{
    public class ShareTechnologyMessageSender : SubSystem<ShareTechnologySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendTechnologyMessage(RDTech tech)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressTechnologyMsgData>();
            msgData.TechNode.Id = tech.techID;

            var configNode = ConvertTechNodeToConfigNode(tech);
            if (configNode == null) return;

            var data = configNode.Serialize();
            var numBytes = data.Length;

            msgData.TechNode.NumBytes = numBytes;
            if (msgData.TechNode.Data.Length < numBytes)
                msgData.TechNode.Data = new byte[numBytes];

            Array.Copy(data, msgData.TechNode.Data, numBytes);

            SendMessage(msgData);
        }

        private static ConfigNode ConvertTechNodeToConfigNode(RDTech techNode)
        {
            var configNode = new ConfigNode();
            try
            {
                configNode.AddValue("id", techNode.techID);
                configNode.AddValue("state", techNode.state);
                configNode.AddValue("cost", techNode.scienceCost);

                foreach (var part in techNode.partsPurchased)
                {
                    configNode.AddValue("part", part.name);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving tech node: {e}");
                return null;
            }

            return configNode;
        }
    }
}
