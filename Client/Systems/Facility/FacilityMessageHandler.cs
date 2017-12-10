using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Facility;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Upgradeables;
using Object = UnityEngine.Object;

namespace LunaClient.Systems.Facility
{
    public class FacilityMessageHandler : SubSystem<FacilitySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is FacilityBaseMsgData msgData)) return;

            switch (msgData.FacilityMessageType)
            {
                case FacilityMessageType.Upgrade:
                    var upgradeMsg = (FacilityUpgradeMsgData)msgData;
                    var upgrFacility = Object.FindObjectsOfType<UpgradeableFacility>().FirstOrDefault(o => o.id == upgradeMsg.ObjectId);
                    if (upgrFacility != null)
                    {
                        upgrFacility.SetLevel(upgradeMsg.Level);
                    }
                    break;
                case FacilityMessageType.Repair:
                case FacilityMessageType.Collapse:
                    var destructibleFacility = Object.FindObjectsOfType<DestructibleBuilding>().FirstOrDefault(o => o.id == msgData.ObjectId);
                    if (destructibleFacility != null)
                    {
                        switch (msgData.FacilityMessageType)
                        {
                            case FacilityMessageType.Repair:
                                destructibleFacility.Repair();
                                break;
                            case FacilityMessageType.Collapse:
                                destructibleFacility.Demolish();
                                break;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}