using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Utilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using System;
using System.Collections.Generic;

namespace LmpClient.Systems.ShareAchievements
{
    public class ShareAchievementsMessageSender : SubSystem<ShareAchievementsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }

        public void SendAchievementsMessage(ProgressNode[] achievements)
        {
            //Convert the achievements to AchievementInfo's.
            var achievementInfos = new List<AchievementInfo>();
            foreach (var achievement in achievements)
            {
                var configNode = ConvertAchievementToConfigNode(achievement);
                if (configNode == null) break;

                var data = ConfigNodeSerializer.Serialize(configNode);
                var numBytes = data.Length;

                achievementInfos.Add(new AchievementInfo
                {
                    Id = achievement.Id,
                    Data = data,
                    NumBytes = numBytes
                });
            }

            //Build the packet and send it.
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressAchievementsMsgData>();
            msgData.Achievements = achievementInfos.ToArray();
            msgData.AchievementsCount = msgData.Achievements.Length;
            System.MessageSender.SendMessage(msgData);
        }

        public void SendAchievementsMessage(ProgressNode achievement)
        {
            SendAchievementsMessage(new[] { achievement });
        }

        private static ConfigNode ConvertAchievementToConfigNode(ProgressNode achievement)
        {
            var configNode = new ConfigNode();
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
