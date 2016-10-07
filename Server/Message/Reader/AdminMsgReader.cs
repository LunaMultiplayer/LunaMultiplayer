using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;

namespace LunaServer.Message.Reader
{
    public class AdminMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var data = (AdminBaseMsgData)messageData;
            switch (data.AdminMessageType)
            {
                case AdminMessageType.LIST_REQUEST:
                    AdminSystemSender.SendAdminList(client);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}