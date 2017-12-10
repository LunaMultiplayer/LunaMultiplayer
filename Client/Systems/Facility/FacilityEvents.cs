using LunaClient.Base;
using LunaClient.VesselUtilities;
using Upgradeables;

namespace LunaClient.Systems.Facility
{
    public class FacilityEvents : SubSystem<FacilitySystem>
    {
        public void FacilityUpgraded(UpgradeableFacility building, int level)
        {
            System.MessageSender.SendFacilityUpgradeMsg(building.id, level);
        }

        public void FacilityRepaired(DestructibleBuilding building)
        {
            System.MessageSender.SendFacilityRepairMsg(building.id);
        }

        public void FacilityCollapsing(DestructibleBuilding building)
        {
            if (!VesselCommon.IsSpectating)
            {
                System.MessageSender.SendFacilityCollapseMsg(building.id);
            }
        }
    }
}
