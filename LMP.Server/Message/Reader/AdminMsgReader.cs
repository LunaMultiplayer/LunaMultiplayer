using LMP.Server.Client;
using LMP.Server.Message.Reader.Base;
using LMP.Server.System;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;

namespace LMP.Server.Message.Reader
{
    public class AdminMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var data = (AdminBaseMsgData)messageData;
            switch (data.AdminMessageType)
            {
                case AdminMessageType.ListRequest:
                    AdminSystemSender.SendAdminList(client);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}