using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
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
            [(ushort)CraftMessageType.ListReply] = typeof(CraftLibraryListReplyMsgData),
            [(ushort)CraftMessageType.RequestFile] = typeof(CraftLibraryRequestMsgData),
            [(ushort)CraftMessageType.RespondFile] = typeof(CraftLibraryRespondMsgData),
            [(ushort)CraftMessageType.UploadFile] = typeof(CraftLibraryUploadMsgData),
            [(ushort)CraftMessageType.AddFile] = typeof(CraftLibraryAddMsgData),
            [(ushort)CraftMessageType.DeleteFile] = typeof(CraftLibraryDeleteMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}