using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class CraftLibraryCliMsg : CliMsgBase<CraftLibraryBaseMsgData>
    {
        /// <inheritdoc />
        internal CraftLibraryCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(CraftLibraryCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)CraftMessageType.FoldersRequest] = typeof(CraftLibraryFoldersRequestMsgData),
            [(ushort)CraftMessageType.ListRequest] = typeof(CraftLibraryListRequestMsgData),
            [(ushort)CraftMessageType.DownloadRequest] = typeof(CraftLibraryDownloadRequestMsgData),
            [(ushort)CraftMessageType.DeleteRequest] = typeof(CraftLibraryDeleteRequestMsgData),
            [(ushort)CraftMessageType.CraftData] = typeof(CraftLibraryDataMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
