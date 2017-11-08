using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using System;

namespace LunaServer.Message.Reader
{
    public class SyncTimeRequestMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (SyncTimeRequestMsgData) message;

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<SyncTimeReplyMsgData>();
            msgData.ClientSendTime = data.ClientSendTime;
            msgData.ServerReceiveTime = DateTime.UtcNow.Ticks;
            msgData.ServerStartTime = ServerContext.StartTime;

            MessageQueuer.SendToClient<SyncTimeSrvMsg>(client, msgData);
        }
    }
}