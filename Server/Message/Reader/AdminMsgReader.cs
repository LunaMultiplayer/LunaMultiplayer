using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class AdminMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (AdminBaseMsgData)message.Data;
            switch (data.AdminMessageType)
            {
                case AdminMessageType.ListRequest:
                    AdminSystemSender.SendAdminList(client);

                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}