using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
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
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            
            for (var i = 0; i < vessel.protoVessel.protoPartSnapshots.Count; i++)
            {
                for (var j = 0; j < vessel.protoVessel.protoPartSnapshots[i].resources.Count; j++)
                {
                    var resource = vessel.protoVessel.protoPartSnapshots[i].resources[j].resourceRef;

                    if(resource == null) continue;

                    var resourceInfo = new VesselResourceInfo
                    {
                        ResourceName = resource.resourceName,
                        PartFlightId = vessel.protoVessel.protoPartSnapshots[i].flightID,
                        Amount = resource.amount,
                        FlowState = resource.flowState
                    };
                    
                    _resources.Add(resourceInfo);
                }
            }
            
            msgData.Resources = _resources.ToArray();
            SendMessage(msgData);
        }
    }
}
