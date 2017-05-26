using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class FlagCliMsg : CliMsgBase<FlagBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)FlagMessageType.List] = new FlagListMsgData(),
            [(ushort)FlagMessageType.FlagData] = new FlagDataMsgData(),
            [(ushort)FlagMessageType.UploadFile] = new FlagUploadMsgData(),
            [(ushort)FlagMessageType.DeleteFile] = new FlagDeleteMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.Flag;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}