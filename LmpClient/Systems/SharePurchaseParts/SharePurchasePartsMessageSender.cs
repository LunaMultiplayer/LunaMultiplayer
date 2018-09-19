using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.SharePurchaseParts
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
