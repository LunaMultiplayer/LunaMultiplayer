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

namespace LunaClient.Systems.ShareScience
{
    public class ShareScienceMessageHandler : SubSystem<ShareScienceSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ScienceUpdate) return;

            if (msgData is ShareProgressScienceMsgData data)
            {
                var science = data.Science; //create a copy of the science value so it will not change in the future.
                LunaLog.Log("Queue ScienceUpdate with: " + science);
                System.QueueAction(() =>
                {
                    ScienceUpdate(science);
                });
            }
        }

        private static void ScienceUpdate(float science)
        {
            System.StartIgnoringEvents();
            ResearchAndDevelopment.Instance.SetScience(science, TransactionReasons.None);
            System.StopIgnoringEvents();
            LunaLog.Log("ScienceUpdate received - science changed to: " + science);
        }
    }
}
