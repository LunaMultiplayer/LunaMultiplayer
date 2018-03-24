using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Network;
using LunaCommon.Message.Data.ShareProgress;

namespace LunaClient.Systems.ShareTechnology
{
    public class ShareTechnologyEvents : SubSystem<ShareTechnologySystem>
    {
        public void TechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (System.IgnoreEvents || data.target != RDTech.OperationResult.Successful) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ShareProgressTechnologyMsgData>();
            msgData.TechId = data.host.techID;
            LunaLog.Log("Technology unlocked: " + msgData.TechId);
            System.MessageSender.SendMessage(msgData);
        }
    }
}
