using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Facility;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using System.Linq;
using Object = UnityEngine.Object;

namespace LunaClient.Systems.Facility
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
                        System.RepairFacilityWithoutSendingMessage(destructibleFacility);
                        break;
                    case FacilityMessageType.Collapse:
                        System.CollapseFacilityWithoutSendingMessage(destructibleFacility);
                        break;
                }
            }
        }
    }
}
