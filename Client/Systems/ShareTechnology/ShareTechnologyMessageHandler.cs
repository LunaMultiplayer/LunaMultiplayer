using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

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
                TechnologyUpdate(data);
            }
        }

        private static void TechnologyUpdate(ShareProgressTechnologyMsgData data)
        {
            System.StartIgnoringEvents();
            var nodes = AssetBase.RnDTechTree.GetTreeTechs();
            foreach (var n in nodes)
            {
                if (n.techID == data.TechId)
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(n);
            }

            ResearchAndDevelopment.RefreshTechTreeUI();
            System.StopIgnoringEvents();
            LunaLog.Log("TechnologyUpdate received - technology unlocked: " + data.TechId);
        }
    }
}
