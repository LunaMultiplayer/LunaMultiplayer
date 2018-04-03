using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.ShareTechnology
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

            var data = ConfigNodeSerializer.Serialize(configNode);
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
                var parentNode = configNode.AddNode("Tech");
                parentNode.AddValue("id", techNode.techID);
                parentNode.AddValue("state", techNode.state);
                parentNode.AddValue("cost", techNode.scienceCost);

                foreach (var part in techNode.partsPurchased)
                {
                    parentNode.AddValue("part", part.name);
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
