using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Admin
{
    public class AdminMessageSender : SubSystem<AdminSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new NotImplementedException("We don't send admin messages!");
        }
    }
}