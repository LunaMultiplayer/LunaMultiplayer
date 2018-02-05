using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselStore;
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

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoResources(msgData);

            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null) return;

            UpdateVesselResources(vessel, msgData);
            UpdateProtoVesselResources(vessel.protoVessel, msgData);
        }
        
        private static void UpdateVesselResources(Vessel vessel, VesselResourceMsgData msgData)
        {
            for (var i = 0; i < msgData.ResourcesCount; i++)
            {
                var resource = msgData.Resources[i];
                var part = VesselCommon.FindPartInVessel(vessel, resource.PartFlightId);
                var partResource = VesselCommon.FindResourceInPart(part, resource.ResourceName);
                if (partResource != null)
                {
                    partResource.amount = resource.Amount;
                }
            }
        }

        private static void UpdateProtoVesselResources(ProtoVessel protoVessel, VesselResourceMsgData msgData)
        {
            if (protoVessel != null)
            {
                for (var i = 0; i < msgData.ResourcesCount; i++)
                {
                    var resource = msgData.Resources[i];
                    
                    var partSnapshot = VesselCommon.FindProtoPartInProtovessel(protoVessel, resource.PartFlightId);
                    var resourceSnapshot = VesselCommon.FindResourceInProtoPart(partSnapshot, resource.ResourceName);
                    if (resourceSnapshot != null)
                    {
                        resourceSnapshot.amount = resource.Amount;
                    }
                }
            }
        }
    }
}
