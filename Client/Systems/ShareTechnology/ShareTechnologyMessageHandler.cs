using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using System.Linq;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologyMessageHandler : SubSystem<ShareTechnologySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.TechnologyUpdate) return;

            if (msgData is ShareProgressTechnologyMsgData data)
            {
                var tech = new TechNodeInfo(data.TechNode); //create a copy of the tech value so it will not change in the future.
                LunaLog.Log($"Queue TechnologyUpdate with: {tech.Id}");
                System.QueueAction(() =>
                {
                    TechnologyUpdate(tech);
                });
            }
        }

        private static void TechnologyUpdate(TechNodeInfo tech)
        {
            System.StartIgnoringEvents();
            var node = AssetBase.RnDTechTree.GetTreeTechs().ToList().Find(n => n.techID == tech.Id);

            var configNode = ConfigNodeSerializer.Deserialize(tech.Data, tech.NumBytes).GetNode("Tech");
            LunaLog.Log("The incoming technology config node looks like:");
            for (var i = 0; i < configNode.CountValues; i++)
            {
                LunaLog.Log($"{configNode.values[i].name}: {configNode.values[i].value}");
            }

            //Unlock the technology
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(node);

            ResearchAndDevelopment.RefreshTechTreeUI();
            System.StopIgnoringEvents();
            LunaLog.Log($"TechnologyUpdate received - technology unlocked / bought: {tech.Id}");
        }
    }
}
