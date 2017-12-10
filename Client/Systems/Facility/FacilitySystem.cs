using LunaClient.Base;

namespace LunaClient.Systems.Facility
{
    public class FacilitySystem : MessageSystem<FacilitySystem, FacilityMessageSender, FacilityMessageHandler>
    {
        #region Fields & Properties

        public FacilityEvents FacilityEvents { get; } = new FacilityEvents();
        
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
    }
}
