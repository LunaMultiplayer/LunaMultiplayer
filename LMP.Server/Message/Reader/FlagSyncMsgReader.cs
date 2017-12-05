using LMP.Server.Client;
using LMP.Server.Message.Reader.Base;
using LMP.Server.System;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LMP.Server.Message.Reader
{
    public class FlagSyncMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (FlagBaseMsgData) message;

            switch (data.FlagMessageType)
            {
                case FlagMessageType.ListRequest:
                    FlagSyncMsgSender.HandleFlagListRequestMessage(client);
                    break;
                case FlagMessageType.FlagDelete:
                    FlagSyncMsgSender.HandleFlagDeleteMessage(client, (FlagDeleteMsgData) message);
                    break;
                case FlagMessageType.FlagData:
                    FlagSyncMsgSender.HandleFlagDataMessage(client, (FlagDataMsgData) message);
                    break;
            }
        }
    }
}