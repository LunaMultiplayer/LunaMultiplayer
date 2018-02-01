using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Facility
{
    public class FacilityUpgradeMsgData : FacilityBaseMsgData
    {
        /// <inheritdoc />
        internal FacilityUpgradeMsgData() { }
        public override FacilityMessageType FacilityMessageType => FacilityMessageType.Upgrade;

        public int Level;

        public override string ClassName { get; } = nameof(FacilityUpgradeMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Level);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Level = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int);
        }
    }
}