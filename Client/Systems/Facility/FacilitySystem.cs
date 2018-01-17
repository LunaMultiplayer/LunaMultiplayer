using LunaClient.Base;
using Upgradeables;

namespace LunaClient.Systems.Facility
{
    public class FacilitySystem : MessageSystem<FacilitySystem, FacilityMessageSender, FacilityMessageHandler>
    {
        #region Fields & Properties

        public FacilityEvents FacilityEvents { get; } = new FacilityEvents();

        public string BuildingIdToIgnore;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(FacilitySystem);

        protected override void OnEnabled()
        {
            GameEvents.OnKSCFacilityUpgraded.Add(FacilityEvents.FacilityUpgraded);
            GameEvents.OnKSCStructureRepaired.Add(FacilityEvents.FacilityRepaired);
            GameEvents.OnKSCStructureCollapsing.Add(FacilityEvents.FacilityCollapsing);
            base.OnEnabled();
        }

        protected override void OnDisabled()
        {
            GameEvents.OnKSCFacilityUpgraded.Remove(FacilityEvents.FacilityUpgraded);
            GameEvents.OnKSCStructureRepaired.Remove(FacilityEvents.FacilityRepaired);
            GameEvents.OnKSCStructureCollapsing.Remove(FacilityEvents.FacilityCollapsing);
            base.OnDisabled();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// If we call facility.SetLevel() then we woud trigger the KSP event and send a message to the server. 
        /// Therefore we add the id to the ignore field so the event FacilityUpgraded doesn't send a network message
        /// </summary>
        public void UpgradeFacilityWithoutSendingMessage(UpgradeableFacility facility, int level)
        {
            BuildingIdToIgnore = facility.id;
            facility.setLevel(level);
            BuildingIdToIgnore = string.Empty;
        }

        /// <summary>
        /// If we call building.Repair() then we woud trigger the KSP event and send a message to the server.         
        /// Therefore we add the id to the ignore field so the event FacilityRepaired doesn't send a network message
        /// </summary>
        public void RepairFacilityWithoutSendingMessage(DestructibleBuilding building)
        {
            if (building == null) return;
            if (building.IsIntact || !building.IsDestroyed)
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
            if (building == null) return;
            if (!building.IsIntact || building.IsDestroyed)
                return;

            BuildingIdToIgnore = building.id;
            building.Demolish();
            BuildingIdToIgnore = string.Empty;
        }

        #endregion
    }
}
