using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class CraftLibraryCliMsg : CliMsgBase<CraftLibraryBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)CraftMessageType.ListRequest] = new CraftLibraryListRequestMsgData(),
            [(ushort)CraftMessageType.RequestFile] = new CraftLibraryRequestMsgData(),
            [(ushort)CraftMessageType.RespondFile] = new CraftLibraryRespondMsgData(),
            [(ushort)CraftMessageType.UploadFile] = new CraftLibraryUploadMsgData(),
            [(ushort)CraftMessageType.AddFile] = new CraftLibraryAddMsgData(),
            [(ushort)CraftMessageType.DeleteFile] = new CraftLibraryDeleteMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.CraftLibrary;
        protected override int DefaultChannel => 9;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}