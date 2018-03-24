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

namespace LunaClient.Systems.ShareFunds
{
    public class ShareFundsMessageHandler : SubSystem<ShareFundsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.FundsUpdate) return;
            
            if (msgData is ShareProgressFundsMsgData data)
            {
                FundsUpdate(data);
            }
        }

        private static void FundsUpdate(ShareProgressFundsMsgData data)
        {
            System.StartIgnoringEvents();
            Funding.Instance.SetFunds(data.Funds, TransactionReasons.None);
            System.StopIgnoringEvents();
            LunaLog.Log("FundsUpdate received - funds changed to: " + data.Funds);
        }
    }
}
