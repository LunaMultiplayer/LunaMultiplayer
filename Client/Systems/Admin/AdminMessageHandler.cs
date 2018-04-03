using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Admin
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
