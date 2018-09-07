using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselResourceSys
{
    public class VesselResourceMessageHandler : SubSystem<VesselResourceSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselResourceMsgData msgData)) return;

            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null) return;
            
            UpdateProtoVesselResources(vessel.protoVessel, msgData);
        }

        private static void UpdateProtoVesselResources(ProtoVessel protoVessel, VesselResourceMsgData msgData)
        {
            if (protoVessel == null) return;

            for (var i = 0; i < msgData.ResourcesCount; i++)
            {
                var resource = msgData.Resources[i];
                    
                var partSnapshot = VesselCommon.FindProtoPartInProtovessel(protoVessel, resource.PartFlightId);
                var resourceSnapshot = VesselCommon.FindResourceInProtoPart(partSnapshot, resource.ResourceName);
                if (resourceSnapshot != null)
                {
                    resourceSnapshot.amount = resource.Amount;
                    resourceSnapshot.flowState = resource.FlowState;

                    if (resourceSnapshot.resourceRef == null) continue;

                    resourceSnapshot.resourceRef.amount = resource.Amount;
                    resourceSnapshot.resourceRef.flowState = resource.FlowState;
                }
            }
        }
    }
}
