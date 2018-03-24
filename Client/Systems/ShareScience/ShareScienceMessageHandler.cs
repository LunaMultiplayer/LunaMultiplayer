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
                ScienceUpdate(data);
            }
        }

        private static void ScienceUpdate(ShareProgressScienceMsgData data)
        {
            System.StartIgnoringEvents();
            ResearchAndDevelopment.Instance.SetScience(data.Science, TransactionReasons.None);
            System.StopIgnoringEvents();
            LunaLog.Log("ScienceUpdate received - science changed to: " + data.Science);
        }
    }
}
