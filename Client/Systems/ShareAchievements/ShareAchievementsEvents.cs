using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;

namespace LunaClient.Systems.ShareAchievements
{
    public class ShareAchievementsEvents : SubSystem<ShareAchievementsSystem>
    {
        #region EventHandlers
        public void AchievementReached(ProgressNode progressNode)
        {
            if (System.IgnoreEvents) return;

            SendAchievementsUpdate(progressNode);
            LunaLog.Log("Achievement reached:" + progressNode.Id);
        }

        public void AchievementCompleted(ProgressNode progressNode)
        {
            if (System.IgnoreEvents) return;

            SendAchievementsUpdate(progressNode);
            LunaLog.Log("Achievement completed:" + progressNode.Id);
        }

        public void AchievementAchieved(ProgressNode progressNode)
        {
            //This event is triggered to often (always if some speed or distance record changes).
            //LunaLog.Log("Achievement achieved:" + progressNode.Id);
        }
        #endregion

        #region PrivateMethods
        private static void SendAchievementsUpdate(ProgressNode[] achievements)
        {
            //Convert the achievements to AchievementInfo's.
            var achievementInfos = new List<AchievementInfo>();
            foreach (var achievement in achievements)
            {
                var configNode = ConvertAchievementToConfigNode(achievement);
                if (configNode == null) break;

                var data = ConfigNodeSerializer.Serialize(configNode);
                var numBytes = data.Length;

                achievementInfos.Add(new AchievementInfo()
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

        private static void SendAchievementsUpdate(ProgressNode achievement)
        {
            SendAchievementsUpdate(new ProgressNode[] { achievement });
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
        #endregion
    }
}
