using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class CraftLibrarySrvMsg : SrvMsgBase<CraftLibraryBaseMsgData>
    {
        /// <inheritdoc />
        internal CraftLibrarySrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)CraftMessageType.ListReply] = MessageStore.GetMessageData<CraftLibraryListReplyMsgData>(true),
            [(ushort)CraftMessageType.RequestFile] = MessageStore.GetMessageData<CraftLibraryRequestMsgData>(true),
            [(ushort)CraftMessageType.RespondFile] = MessageStore.GetMessageData<CraftLibraryRespondMsgData>(true),
            [(ushort)CraftMessageType.UploadFile] = MessageStore.GetMessageData<CraftLibraryUploadMsgData>(true),
            [(ushort)CraftMessageType.AddFile] = MessageStore.GetMessageData<CraftLibraryAddMsgData>(true),
            [(ushort)CraftMessageType.DeleteFile] = MessageStore.GetMessageData<CraftLibraryDeleteMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}