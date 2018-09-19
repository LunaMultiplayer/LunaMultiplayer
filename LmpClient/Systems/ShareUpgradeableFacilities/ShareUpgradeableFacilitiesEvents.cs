using LmpClient.Base;
using Upgradeables;

namespace LmpClient.Systems.ShareUpgradeableFacilities
{
    public class ShareUpgradeableFacilitiesEvents : SubSystem<ShareUpgradeableFacilitiesSystem>
    {
        #region EventHandlers

        public void FacilityUpgraded(UpgradeableFacility facility, int level)
        {
            if (System.IgnoreEvents) return;

            LunaLog.Log($"Facility {facility.id} upgraded to level: {level}");
            System.MessageSender.SendFacilityUpgradeMessage(facility.id, level);
        }

        #endregion
    }
}
