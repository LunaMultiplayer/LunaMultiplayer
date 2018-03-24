using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Network;
using LunaCommon.Message.Data.ShareProgress;

namespace LunaClient.Systems.ShareReputation
{
    public class ShareReputationEvents : SubSystem<ShareReputationSystem>
    {
        public void ReputationChanged(float reputation, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressReputationMsgData>();
            msgData.Reputation = reputation;
            msgData.Reason = reason.ToString();
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log("Reputation changed to: " + reputation + " with reason: " + reason.ToString());
        }
    }
}
