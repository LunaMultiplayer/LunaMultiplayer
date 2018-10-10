using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending facility upgrades.
    /// </summary>
    public class ShareProgressFacilityUpgradeMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressFacilityUpgradeMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.FacilityUpgrade;

        public string FacilityId;
        public int Level;
        public float NormLevel;

        public override string ClassName { get; } = nameof(ShareProgressFacilityUpgradeMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            lidgrenMsg.Write(FacilityId);
            lidgrenMsg.Write(Level);
            lidgrenMsg.Write(NormLevel);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            FacilityId = lidgrenMsg.ReadString();
            Level = lidgrenMsg.ReadInt32();
            NormLevel = lidgrenMsg.ReadFloat();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + FacilityId.GetByteCount() + sizeof(int) + sizeof(float);
        }
    }
}
