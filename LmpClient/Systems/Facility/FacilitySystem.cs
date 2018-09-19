using Harmony;
using LmpClient.Base;
using System.Collections.Generic;

namespace LmpClient.Systems.Facility
{
    /// <summary>
    /// This system syncs when you destroy/repair a facility to other players
    /// </summary>
    public class FacilitySystem : MessageSystem<FacilitySystem, FacilityMessageSender, FacilityMessageHandler>
    {
        #region Fields & Properties

        public FacilityEvents FacilityEvents { get; } = new FacilityEvents();

        public readonly HashSet<string> DestroyedFacilities = new HashSet<string>();

        public string BuildingIdToIgnore;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(FacilitySystem);

        protected override void OnEnabled()
        {
            GameEvents.OnKSCStructureRepairing.Add(FacilityEvents.FacilityRepairing);
            GameEvents.OnKSCStructureCollapsing.Add(FacilityEvents.FacilityCollapsing);

            GameEvents.onFlightReady.Add(FacilityEvents.FlightReady);
            base.OnEnabled();
        }

        protected override void OnDisabled()
        {
            DestroyedFacilities.Clear();

            GameEvents.OnKSCStructureRepaired.Remove(FacilityEvents.FacilityRepairing);
            GameEvents.OnKSCStructureCollapsing.Remove(FacilityEvents.FacilityCollapsing);

            GameEvents.onFlightReady.Remove(FacilityEvents.FlightReady);
            base.OnDisabled();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// If we call building.Repair() then we woud trigger the KSP event and send a message to the server.         
        /// Therefore we add the id to the ignore field so the event FacilityRepaired doesn't send a network message
        /// </summary>
        public void RepairFacilityWithoutSendingMessage(DestructibleBuilding building)
        {
            if (building == null || building.IsIntact || !building.IsDestroyed)
                return;

            BuildingIdToIgnore = building.id;
            building.Repair();
            BuildingIdToIgnore = string.Empty;
        }

        /// <summary>
        /// If we call building.Demolish() then we woud trigger the KSP event and send a message to the server.         
        /// Therefore we add the id to the ignore field so the event FacilityCollapsing doesn't send a network message
        /// </summary>
        public void CollapseFacilityWithoutSendingMessage(DestructibleBuilding building)
        {
            if (building == null || !building.IsIntact || building.IsDestroyed)
                return;

            BuildingIdToIgnore = building.id;
            building.Demolish();
            BuildingIdToIgnore = string.Empty;
        }

        /// <summary>
        /// Collapses a building without replaying the special effects. 
        /// Useful when reverting as you don't want to see the flames again...
        /// </summary>
        public void CollapseFacilityWithoutSfx(DestructibleBuilding building)
        {
            if (building == null || !building.IsIntact || building.IsDestroyed)
                return;

            BuildingIdToIgnore = building.id;
            foreach (var collapsibleObj in building.CollapsibleObjects)
            {
                collapsibleObj.SetDestroyed(true);
            }

            Traverse.Create(building).Field("destroyed").SetValue(true);
            Traverse.Create(building).Field("intact").SetValue(false);
            BuildingIdToIgnore = string.Empty;
        }

        #endregion
    }
}
