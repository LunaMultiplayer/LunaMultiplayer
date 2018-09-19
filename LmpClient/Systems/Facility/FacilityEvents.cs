using LmpClient.Base;
using LmpClient.Systems.Scenario;
using LmpClient.VesselUtilities;
using System.Linq;
using UnityEngine;

namespace LmpClient.Systems.Facility
{
    public class FacilityEvents : SubSystem<FacilitySystem>
    {
        public void FacilityRepairing(DestructibleBuilding building)
        {
            if (System.BuildingIdToIgnore == building.id) return;

            System.DestroyedFacilities.Remove(building.id);
            System.MessageSender.SendFacilityRepairMsg(building.id);
            ScenarioSystem.Singleton.SendScenarioModules();
        }

        public void FacilityCollapsing(DestructibleBuilding building)
        {
            if (System.BuildingIdToIgnore == building.id) return;

            //Don't send this kind of messages when spectating as they are not accurate
            if (!VesselCommon.IsSpectating)
            {
                System.DestroyedFacilities.Add(building.id);
                System.MessageSender.SendFacilityCollapseMsg(building.id);
                ScenarioSystem.Singleton.SendScenarioModules();
            }
        }

        /// <summary>
        /// This event is called just when we start flying. We don't suport REVERT for facilities so we set them destroyed if they were destroyed.
        /// </summary>
        public void FlightReady()
        {
            var buildings = Object.FindObjectsOfType<DestructibleBuilding>().Where(b=> !b.IsDestroyed);
            foreach (var building in buildings)
            {
                if (System.DestroyedFacilities.Contains(building.id))
                {
                    System.CollapseFacilityWithoutSfx(building);
                }
            }
        }
    }
}
