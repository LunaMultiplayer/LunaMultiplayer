using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryRespondMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryRespondMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.RespondFile;

        public string CraftOwner;
        public CraftType RequestedType;
        public bool HasCraft;

        public int NumBytes;
        public byte[] CraftData = new byte[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(CraftOwner);
            lidgrenMsg.Write((int)RequestedType);
            lidgrenMsg.Write(HasCraft);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(CraftData, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            CraftOwner = lidgrenMsg.ReadString();
            RequestedType = (CraftType)lidgrenMsg.ReadInt32();
            HasCraft = lidgrenMsg.ReadBoolean();

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
            return base.InternalGetMessageSize(dataCompressed) + CraftOwner.GetByteCount() + sizeof(CraftType) + sizeof(bool) + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}