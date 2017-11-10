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
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)CraftMessageType.ListRequest] = typeof(CraftLibraryListRequestMsgData),
            [(ushort)CraftMessageType.RequestFile] = typeof(CraftLibraryRequestMsgData),
            [(ushort)CraftMessageType.RespondFile] = typeof(CraftLibraryRespondMsgData),
            [(ushort)CraftMessageType.UploadFile] = typeof(CraftLibraryUploadMsgData),
            [(ushort)CraftMessageType.AddFile] = typeof(CraftLibraryAddMsgData),
            [(ushort)CraftMessageType.DeleteFile] = typeof(CraftLibraryDeleteMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}