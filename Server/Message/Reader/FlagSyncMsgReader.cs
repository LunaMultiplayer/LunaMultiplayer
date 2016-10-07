using LunaCommon.Enums;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;

namespace LunaServer.Message.Reader
{
    public class FlagSyncMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (FlagBaseMsgData) message;
            if (data.PlayerName != client.PlayerName) return;

            switch (data.FlagMessageType)
            {
                case FlagMessageType.LIST:
                    FlagSyncMsgSender.HandleListFlagMessage(client, (FlagListMsgData) message);
                    break;
                case FlagMessageType.DELETE_FILE:
                    FlagSyncMsgSender.HandleDeleteFlagMessage(client, (FlagDeleteMsgData) message);
                    break;
                case FlagMessageType.UPLOAD_FILE:
                    FlagSyncMsgSender.HandleUploadFlagMessage(client, (FlagUploadMsgData) message);
                    break;
            }
        }
    }
}