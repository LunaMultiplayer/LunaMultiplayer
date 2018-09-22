using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending achievements between clients.
    /// </summary>
    public class ShareProgressAchievementsMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressAchievementsMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.AchievementsUpdate;

        public string Id;
        public int NumBytes;
        public byte[] Data = new byte[0];

        public override string ClassName { get; } = nameof(ShareProgressAchievementsMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(Data, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Id = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (Data.Length < NumBytes)
                Data = new byte[NumBytes];

            lidgrenMsg.ReadBytes(Data, 0, NumBytes);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + sizeof(byte) * NumBytes;
        }
    }
}
