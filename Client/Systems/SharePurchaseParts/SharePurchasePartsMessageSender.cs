using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.SharePurchaseParts
{
    public class SharePurchasePartsMessageSender : SubSystem<SharePurchasePartsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendPartPurchasedMessage(string techId, string partName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressPartPurchaseMsgData>();
            msgData.PartName = partName;
            msgData.TechId = techId;

            SendMessage(msgData);
        }
    }
}
