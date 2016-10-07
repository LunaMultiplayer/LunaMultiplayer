using System;
using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class CraftLibrarySrvMsg : SrvMsgBase<CraftLibraryBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)CraftMessageType.LIST_REPLY] = new CraftLibraryListReplyMsgData(),
            [(ushort)CraftMessageType.REQUEST_FILE] = new CraftLibraryRequestMsgData(),
            [(ushort)CraftMessageType.RESPOND_FILE] = new CraftLibraryRespondMsgData(),
            [(ushort)CraftMessageType.UPLOAD_FILE] = new CraftLibraryUploadMsgData(),
            [(ushort)CraftMessageType.ADD_FILE] = new CraftLibraryAddMsgData(),
            [(ushort)CraftMessageType.DELETE_FILE] = new CraftLibraryDeleteMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.CRAFT_LIBRARY;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}