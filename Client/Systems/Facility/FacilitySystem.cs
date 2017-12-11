using LunaClient.Base;
using System.Reflection;
using Upgradeables;

namespace LunaClient.Systems.Facility
{
    public class FacilitySystem : MessageSystem<FacilitySystem, FacilityMessageSender, FacilityMessageHandler>
    {
        #region Fields & Properties

        public FacilityEvents FacilityEvents { get; } = new FacilityEvents();

        #region Reflection fields

        private static readonly FieldInfo IntactField = typeof(DestructibleBuilding).GetField("intact", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo DestroyedField = typeof(DestructibleBuilding).GetField("destroyed", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion

        #endregion

        #region Base overrides

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
        /// If we call facility.SetLevel() then we woud trigger the KSP event and send a message to the server. Here we just do the same logic
        /// as KSP but without triggering the event
        /// </summary>
        public void UpgradeFacilityWithoutTriggeringEvent(UpgradeableFacility facility, int level)
        {
            facility.setLevel(level);
        }

        /// <summary>
        /// If we call building.Repair() then we woud trigger the KSP event and send a message to the server. Here we just do the same logic
        /// as KSP but without triggering the event
        /// </summary>
        public void RepairFacilityWithoutTriggeringEvent(DestructibleBuilding building)
        {
            if ((bool) IntactField.GetValue(building) || !(bool)DestroyedField.GetValue(building))
                return;

            foreach (var objectCollapsible in building.CollapsibleObjects)
            {
                objectCollapsible.Repair(building);
            }

            IntactField.SetValue(building, true);
            DestroyedField.SetValue(building, false);
        }

        /// <summary>
        /// If we call building.Demolish() then we woud trigger the KSP event and send a message to the server. Here we just do the same logic
        /// as KSP but without triggering the event
        /// </summary>
        public void CollapseFacilityWithoutTriggeringEvent(DestructibleBuilding building)
        {
            if (!(bool)IntactField.GetValue(building) || (bool)DestroyedField.GetValue(building))
                return;

            foreach (var objectCollapsible in building.CollapsibleObjects)
            {
                objectCollapsible.Collapse(building);
            }

            IntactField.SetValue(building, false);
            DestroyedField.SetValue(building, true);
        }

        #endregion
    }
}
