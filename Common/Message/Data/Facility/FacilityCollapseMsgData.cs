using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Facility
{
    public class FacilityCollapseMsgData : FacilityBaseMsgData
    {
        /// <inheritdoc />
        internal FacilityCollapseMsgData() { }
        public override FacilityMessageType FacilityMessageType => FacilityMessageType.Collapse;
    }
}