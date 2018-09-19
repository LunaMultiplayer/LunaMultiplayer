using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.Facility;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System.Collections.Concurrent;
using System.Linq;
using Object = UnityEngine.Object;

namespace LmpClient.Systems.Facility
{
    public class FacilityMessageHandler : SubSystem<FacilitySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is FacilityBaseMsgData msgData)) return;

            var destructibleFacility = Object.FindObjectsOfType<DestructibleBuilding>().FirstOrDefault(o => o.id == msgData.ObjectId);
            if (destructibleFacility != null)
            {
                switch (msgData.FacilityMessageType)
                {
                    case FacilityMessageType.Repair:
                        System.DestroyedFacilities.Remove(destructibleFacility.id);
                        System.RepairFacilityWithoutSendingMessage(destructibleFacility);
                        break;
                    case FacilityMessageType.Collapse:
                        System.DestroyedFacilities.Add(destructibleFacility.id);
                        System.CollapseFacilityWithoutSendingMessage(destructibleFacility);
                        break;
                }
            }
        }
    }
}
