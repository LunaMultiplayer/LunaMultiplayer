using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.ShareUpgradeableFacilities
{
    public class ShareUpgradeableFacilitiesMessageSender : SubSystem<ShareUpgradeableFacilitiesSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendFacilityUpgradeMessage(string facilityId, int level)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressFacilityUpgradeMsgData>();
            msgData.FacilityId = facilityId;
            msgData.Level = level;
            System.MessageSender.SendMessage(msgData);
        }
    }
}
