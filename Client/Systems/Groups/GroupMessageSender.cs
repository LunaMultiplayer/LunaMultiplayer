using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base.Interface;
using LunaCommon.Message.Interface;
using LunaClient.Base;

namespace LunaClient.Systems.Groups
{
    class GroupMessageSender : SubSystem<GroupSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new NotImplementedException();
        }
    }
}
