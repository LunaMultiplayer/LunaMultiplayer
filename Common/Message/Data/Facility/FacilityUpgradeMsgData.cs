using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Facility
{
    public class FacilityUpgradeMsgData : FacilityBaseMsgData
    {
        /// <inheritdoc />
        internal FacilityUpgradeMsgData() { }
        public override FacilityMessageType FacilityMessageType => FacilityMessageType.Upgrade;
        public int Level { get; set; }
    }
}