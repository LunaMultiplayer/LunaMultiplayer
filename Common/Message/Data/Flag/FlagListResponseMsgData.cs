using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagListResponseMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagListResponseMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.ListResponse;

        public int FlagCount;
        public FlagInfo[] FlagFiles = new FlagInfo[0];

        public override string ClassName { get; } = nameof(FlagListResponseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(FlagCount);
            for (var i = 0; i < FlagCount; i++)
            {
                FlagFiles[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            FlagCount = lidgrenMsg.ReadInt32();
            for (var i = 0; i < FlagCount; i++)
            {
                if(FlagFiles[i] == null)
                    FlagFiles[i] = new FlagInfo();

                FlagFiles[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < FlagCount; i++)
            {
                arraySize += FlagFiles[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}