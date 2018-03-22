using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Generic;

namespace LunaClient.Systems.VesselResourceSys
{
    public class VesselResourceMessageSender : SubSystem<VesselResourceSystem>, IMessageSender
    {
        private static List<VesselResourceInfo> _resources = new List<VesselResourceInfo>();

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselResources(Vessel vessel)
        {
            _resources.Clear();

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselResourceMsgData>();
            msgData.VesselId = vessel.id;
            
            for (var i = 0; i < vessel.protoVessel.protoPartSnapshots.Count; i++)
            {
                for (var j = 0; j < vessel.protoVessel.protoPartSnapshots[i].resources.Count; j++)
                {
                    var protoResource = vessel.protoVessel.protoPartSnapshots[i].resources[j];
                    var resource = vessel.protoVessel.protoPartSnapshots[i].resources[j].resourceRef;

                    if(resource == null) continue;

                    var resourceInfo = new VesselResourceInfo
                    {
                        ResourceName = resource.resourceName,
                        PartFlightId = vessel.protoVessel.protoPartSnapshots[i].flightID,
                        Amount = resource.amount,
                        FlowState = resource.flowState
                    };

                    //Now update the proto values our vessel...
                    protoResource.amount = resource.amount;
                    protoResource.flowState = resource.flowState;

                    _resources.Add(resourceInfo);
                }
            }
            
            msgData.Resources = _resources.ToArray();

            //Update our own values in the store aswell as otherwise if we leave the vessel it can still be in the safety bubble
            VesselsProtoStore.UpdateVesselProtoResources(msgData);

            SendMessage(msgData);
        }
    }
}
