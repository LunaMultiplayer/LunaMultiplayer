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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(Level);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Level = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int);
        }
    }
}