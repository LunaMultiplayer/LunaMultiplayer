using System;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;

namespace Server.Message.Reader
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