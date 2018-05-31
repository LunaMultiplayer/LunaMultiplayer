using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselRemoveMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselRemoveMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Remove;
        
        public bool AddToKillList;

        public override string ClassName { get; } = nameof(VesselRemoveMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);


            lidgrenMsg.Write(AddToKillList);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            

            AddToKillList = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(bool);
        }
    }
}
