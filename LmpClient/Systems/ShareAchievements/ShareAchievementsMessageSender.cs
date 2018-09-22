using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Utilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.ShareAchievements
{
    public class ShareAchievementsMessageSender : SubSystem<ShareAchievementsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendAchievementsMessage(ProgressNode achievement)
        {
            var configNode = ConvertAchievementToConfigNode(achievement);
            if (configNode == null) return;

            //Build the packet and send it.
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressAchievementsMsgData>();
            msgData.Id = achievement.Id;
            msgData.Data = ConfigNodeSerializer.Serialize(configNode);
            msgData.NumBytes = msgData.Data.Length;
            System.MessageSender.SendMessage(msgData);
        }

        private static ConfigNode ConvertAchievementToConfigNode(ProgressNode achievement)
        {
            var configNode = new ConfigNode(achievement.Id);
            try
            {
                achievement.Save(configNode);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while saving achievement: {e}");
                return null;
            }

            return configNode;
        }
    }
}
