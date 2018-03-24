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

namespace LunaClient.Systems.ShareReputation
{
    public class ShareReputationMessageHandler : SubSystem<ShareReputationSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ReputationUpdate) return;

            if (msgData is ShareProgressReputationMsgData data)
            {
                ReputationUpdate(data);
            }
        }

        private static void ReputationUpdate(ShareProgressReputationMsgData data)
        {
            System.StartIgnoringEvents();
            Reputation.Instance.SetReputation(data.Reputation, TransactionReasons.None);
            System.StopIgnoringEvents();
            LunaLog.Log("ReputationUpdate received - reputation changed to: " + data.Reputation);
        }
    }
}
