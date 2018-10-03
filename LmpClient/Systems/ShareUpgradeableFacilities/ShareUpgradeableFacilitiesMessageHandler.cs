using System.Collections.Concurrent;
using System.Linq;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using UnityEngine;
using Upgradeables;

namespace LmpClient.Systems.ShareUpgradeableFacilities
{
    public class ShareUpgradeableFacilitiesMessageHandler : SubSystem<ShareUpgradeableFacilitiesSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.FacilityUpgrade) return;

            if (msgData is ShareProgressFacilityUpgradeMsgData data)
            {
                var facilityId = data.FacilityId;
                var level = data.Level;
                LunaLog.Log("Queue FacilityLevelUpdate.");
                System.QueueAction(() =>
                {
                    FacilityLevelUpdate(facilityId, level);
                });
            }
        }

        private static void FacilityLevelUpdate(string facilityId, int newLevel)
        {
            System.StartIgnoringEvents();
            
            var facility = Object.FindObjectsOfType<UpgradeableFacility>().FirstOrDefault(o => o.id == facilityId);
            if (facility != null)
            {
                facility.SetLevel(newLevel);
            }

            //Listen to the events again.
            System.StopIgnoringEvents();
        }
    }
}
