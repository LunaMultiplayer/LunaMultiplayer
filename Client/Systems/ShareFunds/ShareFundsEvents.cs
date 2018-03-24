using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Network;
using LunaCommon.Message.Data.ShareProgress;

namespace LunaClient.Systems.ShareFunds
{
    public class ShareFundsEvents : SubSystem<ShareFundsSystem>
    {
        public void FundsChanged(double funds, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressFundsMsgData>();
            msgData.Funds = funds;
            msgData.Reason = reason.ToString();
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log("Funds changed to: " + funds + " with reason: " + reason.ToString());
        }
    }
}
