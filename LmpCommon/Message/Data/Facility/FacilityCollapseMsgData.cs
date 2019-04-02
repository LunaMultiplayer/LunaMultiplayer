using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Facility
{
    public class FacilityCollapseMsgData : FacilityBaseMsgData
    {
        /// <inheritdoc />
        internal FacilityCollapseMsgData() { }
        public override FacilityMessageType FacilityMessageType => FacilityMessageType.Collapse;

        public override string ClassName { get; } = nameof(FacilityCollapseMsgData);
    }
}