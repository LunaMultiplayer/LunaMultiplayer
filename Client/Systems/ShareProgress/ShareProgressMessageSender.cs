using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaClient.Systems.ShareProgress
{
    class ShareProgressMessageSender : SubSystem<ShareProgressSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }
    }
}
