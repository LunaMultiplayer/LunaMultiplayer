using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.ShareContracts
{
    public class ShareContractsMessageSender : SubSystem<ShareContractsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ShareProgressCliMsg>(msg)));
        }
    }
}
