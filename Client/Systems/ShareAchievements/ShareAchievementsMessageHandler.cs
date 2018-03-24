using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.ShareFunds;
using LunaClient.Systems.ShareReputation;
using LunaClient.Systems.ShareScience;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaClient.Systems.ShareAchievements
{
    public class ShareAchievementsMessageHandler : SubSystem<ShareAchievementsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.AchievementsUpdate) return;

            if (msgData is ShareProgressAchievementsMsgData data)
            {
                AchievementUpdate(data);
            }
        }

        private static void AchievementUpdate(ShareProgressAchievementsMsgData data)
        {
            System.StartIgnoringEvents();
            ShareFundsSystem.Singleton.StartIgnoringEvents();
            ShareScienceSystem.Singleton.StartIgnoringEvents();
            ShareReputationSystem.Singleton.StartIgnoringEvents();

            foreach (var mInfo in data.Achievements)
            {
                var incomingAchievement = ConvertByteArrayToAchievement(mInfo.Data, mInfo.NumBytes, mInfo.Id);
                if (incomingAchievement == null) continue;

                var achievementIndex = -1;
                for (var i = 0; i < ProgressTracking.Instance.achievementTree.Count; i++)
                {
                    if (ProgressTracking.Instance.achievementTree[i].Id != incomingAchievement.Id) continue;
                    achievementIndex = i;
                    break;
                }

                if (achievementIndex != -1)
                {
                    //found the same achievement in the achievementTree
                    if (!ProgressTracking.Instance.achievementTree[achievementIndex].IsReached && incomingAchievement.IsReached)
                        ProgressTracking.Instance.achievementTree[achievementIndex].Reach();

                    if (!ProgressTracking.Instance.achievementTree[achievementIndex].IsComplete && incomingAchievement.IsComplete)
                        ProgressTracking.Instance.achievementTree[achievementIndex].Complete();

                    LunaLog.Log("Achievement was updated: " + incomingAchievement.Id);
                }
                else
                {
                    //didn't found the same achievement in the achievmentTree
                    ProgressTracking.Instance.achievementTree.AddNode(incomingAchievement);
                    LunaLog.Log("Achievement was added: " + incomingAchievement.Id);
                }
            }

            //Listen to the events again.
            //Restore funds, science and reputation in case the achievement action changed some of that.
            ShareFundsSystem.Singleton.StopIgnoringEvents(true);
            ShareScienceSystem.Singleton.StopIgnoringEvents(true);
            ShareReputationSystem.Singleton.StopIgnoringEvents(true);
            System.StopIgnoringEvents();
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode and then to a ProgressNode.
        /// If anything goes wrong it will return null.
        /// </summary>
        /// <param name="data">The byte array that represents the configNode</param>
        /// <param name="numBytes">The length of the byte array</param>
        /// <param name="progressNodeId">The Id of the ProgressNode</param>
        /// <returns></returns>
        private static ProgressNode ConvertByteArrayToAchievement(byte[] data, int numBytes, string progressNodeId)
        {
            ConfigNode node;
            try
            {
                node = ConfigNodeSerializer.Deserialize(data, numBytes);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing achievement configNode: {e}");
                return null;
            }

            if (node == null)
            {
                LunaLog.LogError("[LMP]: Error, the achievement configNode was null.");
                return null;
            }

            ProgressNode achievement;
            try
            {
                achievement = new ProgressNode(progressNodeId, false);
                achievement.Load(node);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing achievement: {e}");
                return null;
            }

            return achievement;
        }
    }
}
