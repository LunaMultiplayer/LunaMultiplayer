using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Network;
using LunaCommon.Message.Data.ShareProgress;

namespace LunaClient.Systems.ShareScience
{
    public class ShareScienceEvents : SubSystem<ShareScienceSystem>
    {
        public void ScienceChanged(float science, TransactionReasons reason)
        {
            if (System.IgnoreEvents) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressScienceMsgData>();
            msgData.Science = science;
            msgData.Reason = reason.ToString();
            System.MessageSender.SendMessage(msgData);
            LunaLog.Log("Science changed to: " + science + " with reason: " + reason.ToString());
        }
    }
}
