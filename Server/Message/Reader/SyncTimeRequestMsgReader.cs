using System;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;

namespace LunaServer.Message.Reader
{
    public class SyncTimeRequestMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (SyncTimeRequestMsgData) message;

            var newMessageData = new SyncTimeReplyMsgData
            {
                ClientSendTime = data.ClientSendTime,
                ServerReceiveTime = DateTime.UtcNow.Ticks,
                ServerStartTime = ServerContext.StartTime
            };

            MessageQueuer.SendToClient<SyncTimeSrvMsg>(client, newMessageData);
        }
    }
}