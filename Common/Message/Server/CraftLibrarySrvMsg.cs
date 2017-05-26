using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class CraftLibrarySrvMsg : SrvMsgBase<CraftLibraryBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)CraftMessageType.ListReply] = new CraftLibraryListReplyMsgData(),
            [(ushort)CraftMessageType.RequestFile] = new CraftLibraryRequestMsgData(),
            [(ushort)CraftMessageType.RespondFile] = new CraftLibraryRespondMsgData(),
            [(ushort)CraftMessageType.UploadFile] = new CraftLibraryUploadMsgData(),
            [(ushort)CraftMessageType.AddFile] = new CraftLibraryAddMsgData(),
            [(ushort)CraftMessageType.DeleteFile] = new CraftLibraryDeleteMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}