using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

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
                var techId = data.TechId; //create a copy of the techId value so it will not change in the future.
                LunaLog.Log($"Queue TechnologyUpdate with: {techId}");
                System.QueueAction(() =>
                {
                    TechnologyUpdate(techId);
                });
            }
        }

        private static void TechnologyUpdate(string techId)
        {
            System.StartIgnoringEvents();
            var nodes = AssetBase.RnDTechTree.GetTreeTechs();
            foreach (var n in nodes)
            {
                if (n.techID == techId)
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(n);
            }

            ResearchAndDevelopment.RefreshTechTreeUI();
            System.StopIgnoringEvents();
            LunaLog.Log($"TechnologyUpdate received - technology unlocked: {techId}");
        }
    }
}
