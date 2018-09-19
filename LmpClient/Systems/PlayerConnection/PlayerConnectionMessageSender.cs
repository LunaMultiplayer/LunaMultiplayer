using System;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.PlayerConnection
{
    public class PlayerConnectionMessageSender : SubSystem<PlayerConnectionSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new Exception("We don't send this messages!");
        }
    }
}