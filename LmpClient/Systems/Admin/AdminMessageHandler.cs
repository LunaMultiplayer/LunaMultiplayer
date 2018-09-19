using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.Admin;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.Admin
{
    public class AdminMessageHandler : SubSystem<AdminSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is AdminBaseMsgData msgData)) return;

            switch (msgData.AdminMessageType)
            {
                case AdminMessageType.Reply:
                    LunaScreenMsg.PostScreenMessage($"Admin command reply: {((AdminReplyMsgData)msgData).Response}", 5f, ScreenMessageStyle.UPPER_RIGHT);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
