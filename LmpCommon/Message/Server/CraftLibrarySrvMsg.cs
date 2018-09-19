using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.CraftLibrary;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class CraftLibrarySrvMsg : SrvMsgBase<CraftLibraryBaseMsgData>
    {
        /// <inheritdoc />
        internal CraftLibrarySrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(CraftLibrarySrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)CraftMessageType.FoldersReply] = typeof(CraftLibraryFoldersReplyMsgData),
            [(ushort)CraftMessageType.ListReply] = typeof(CraftLibraryListReplyMsgData),
            [(ushort)CraftMessageType.DeleteRequest] = typeof(CraftLibraryDeleteRequestMsgData),
            [(ushort)CraftMessageType.CraftData] = typeof(CraftLibraryDataMsgData),
            [(ushort)CraftMessageType.Notification] = typeof(CraftLibraryNotificationMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
