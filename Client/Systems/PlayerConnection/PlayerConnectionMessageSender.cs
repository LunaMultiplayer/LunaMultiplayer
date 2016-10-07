using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.PlayerConnection
{
    public class PlayerConnectionMessageSender : SubSystem<PlayerConnectionSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new Exception("We don't send this messages!");
        }
    }
}