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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(FlagCount);
            for (var i = 0; i < FlagCount; i++)
            {
                FlagFiles[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            FlagCount = lidgrenMsg.ReadInt32();
            for (var i = 0; i < FlagCount; i++)
            {
                if(FlagFiles[i] == null)
                    FlagFiles[i] = new FlagInfo();

                FlagFiles[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < FlagCount; i++)
            {
                arraySize += FlagFiles[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}