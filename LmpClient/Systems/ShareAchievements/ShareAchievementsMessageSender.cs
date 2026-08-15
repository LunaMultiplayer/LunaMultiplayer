using HarmonyLib;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Network;
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

        public void SendAchievementsMessage(ProgressNode achievement)
        {
            //We only send the ProgressNodes that are CelestialBodySubtree
            var foundNode = ProgressTracking.Instance.FindNode(achievement.Id);
            if (foundNode == null)
            {
                var traverse = new Traverse(achievement).Field<CelestialBody>("body");

                var body = traverse.Value ? traverse.Value.name : null;
                if (body != null)
                {
                    foundNode = ProgressTracking.Instance.FindNode(body);
                }
            }

            if (foundNode != null)
            {
                var configNode = ConvertAchievementToConfigNode(foundNode);
                if (configNode == null) return;

                //Build the packet and send it.
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressAchievementsMsgData>();
                msgData.Id = foundNode.Id;
                msgData.Data = configNode.Serialize();
                msgData.NumBytes = msgData.Data.Length;
                System.MessageSender.SendMessage(msgData);
            }
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

            // KSP appends to ProgressNode.crew every time a milestone is touched and never
            // dedupes. Across many sessions / reverts the list grows without bound, which then
            // gets relayed to every other client and persisted server-side. Trim it before we
            // put it on the wire so we don't propagate (or amplify) the bloat that causes the
            // multi-second scene-transition stalls in issue #542.
            DedupeCrewLists(configNode);

            return configNode;
        }

        /// <summary>
        /// Recursively removes duplicate <c>item = &lt;name&gt;</c> values inside any <c>crew</c>
        /// sub-node of <paramref name="node"/>. First occurrence wins; non-<c>item</c> values
        /// inside a crew node are preserved.
        /// </summary>
        private static void DedupeCrewLists(ConfigNode node)
        {
            if (node == null) return;

            foreach (var child in node.GetNodes())
            {
                if (string.Equals(child.name, CrewSubNodeName, StringComparison.Ordinal))
                {
                    DedupeSingleCrewNode(child);
                    continue;
                }

                DedupeCrewLists(child);
            }
        }

        private static void DedupeSingleCrewNode(ConfigNode crewNode)
        {
            var items = crewNode.GetValues(CrewItemValueKey);
            if (items == null || items.Length <= 1) return;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var unique = new List<string>(items.Length);
            foreach (var item in items)
            {
                if (seen.Add(item)) unique.Add(item);
            }

            if (unique.Count == items.Length) return;

            crewNode.RemoveValues(CrewItemValueKey);
            foreach (var item in unique)
            {
                crewNode.AddValue(CrewItemValueKey, item);
            }
        }

        private const string CrewSubNodeName = "crew";
        private const string CrewItemValueKey = "item";
    }
}
