using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryUploadMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryUploadMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.UploadFile;

        public CraftType UploadType;
        public string UploadName;

        public int NumBytes;
        public byte[] CraftData = new byte[0];

        public override string ClassName { get; } = nameof(CraftLibraryUploadMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write((int)UploadType);
            lidgrenMsg.Write(UploadName);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(CraftData, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            UploadType = (CraftType)lidgrenMsg.ReadInt32();
            UploadName = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            CraftData = ArrayPool<byte>.Claim(NumBytes);
            lidgrenMsg.ReadBytes(CraftData, 0, NumBytes);
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<byte>.Release(ref CraftData);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(CraftType) + UploadName.GetByteCount() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}