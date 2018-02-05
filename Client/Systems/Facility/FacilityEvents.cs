using LunaClient.Base;
using LunaClient.Systems.Scenario;
using LunaClient.VesselUtilities;
using Upgradeables;

namespace LunaClient.Systems.Facility
{
    public class FacilityEvents : SubSystem<FacilitySystem>
    {
        public void FacilityUpgraded(UpgradeableFacility building, int level)
        {
            if (System.BuildingIdToIgnore == building.id) return;

            System.MessageSender.SendFacilityUpgradeMsg(building.id, level);
            SystemsContainer.Get<ScenarioSystem>().SendScenarioModules();
        }

        public void FacilityRepaired(DestructibleBuilding building)
        {
            if (System.BuildingIdToIgnore == building.id) return;

            System.MessageSender.SendFacilityRepairMsg(building.id);
            SystemsContainer.Get<ScenarioSystem>().SendScenarioModules();
        }

        public void FacilityCollapsing(DestructibleBuilding building)
        {
            if (System.BuildingIdToIgnore == building.id) return;

            //Don't send this kind of messages when spectating as they are not accurate
            if (!VesselCommon.IsSpectating)
            {
                System.MessageSender.SendFacilityCollapseMsg(building.id);
                SystemsContainer.Get<ScenarioSystem>().SendScenarioModules();
            }
        }
    }
}
