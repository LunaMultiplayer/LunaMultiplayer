using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data
{
    public class ModMsgData : MessageData
    {
        /// <inheritdoc />
        internal ModMsgData() { }

        /// <summary>
        /// Name of the mod that creates this msg
        /// </summary>
        public string ModName;

        /// <summary>
        /// Relay the msg to all players once it arrives to the serer
        /// </summary>
        public bool Relay;

        /// <summary>
        /// Send it in reliable mode or in UDP-unreliable mode
        /// </summary>
        public bool Reliable;

        /// <summary>
        /// Number of bytes that are being sent
        /// </summary>
        public int NumBytes;

        /// <summary>
        /// Data to send
        /// </summary>
        public byte[] Data;

        public override string ClassName { get; } = nameof(ModMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(ModName);
            lidgrenMsg.Write(Relay);
            lidgrenMsg.Write(Reliable);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            ModName = lidgrenMsg.ReadString();
            Relay = lidgrenMsg.ReadBoolean();
            Reliable = lidgrenMsg.ReadBoolean();
            NumBytes = lidgrenMsg.ReadInt32();

            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return ModName.GetByteCount() + sizeof(bool) + sizeof(bool) + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}