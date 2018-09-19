using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Facility
{
    public class FacilityRepairMsgData : FacilityBaseMsgData
    {
        /// <inheritdoc />
        internal FacilityRepairMsgData() { }
        public override FacilityMessageType FacilityMessageType => FacilityMessageType.Repair;

        public override string ClassName { get; } = nameof(FacilityRepairMsgData);
    }
}