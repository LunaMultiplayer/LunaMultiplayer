using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class FlagCliMsg : CliMsgBase<FlagBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)FlagMessageType.LIST] = new FlagListMsgData(),
            [(ushort)FlagMessageType.FLAG_DATA] = new FlagDataMsgData(),
            [(ushort)FlagMessageType.UPLOAD_FILE] = new FlagUploadMsgData(),
            [(ushort)FlagMessageType.DELETE_FILE] = new FlagDeleteMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.FLAG;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}