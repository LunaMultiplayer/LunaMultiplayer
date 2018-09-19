using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Generic;

namespace LmpClient.Systems.VesselResourceSys
{
    public class VesselResourceMessageSender : SubSystem<VesselResourceSystem>, IMessageSender
    {
        private static readonly List<VesselResourceInfo> Resources = new List<VesselResourceInfo>();

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselResources(Vessel vessel)
        {
            Resources.Clear();

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselResourceMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.VesselPersistentId = vessel.persistentId;
            
            for (var i = 0; i < vessel.protoVessel.protoPartSnapshots.Count; i++)
            {
                for (var j = 0; j < vessel.protoVessel.protoPartSnapshots[i].resources.Count; j++)
                {
                    var resource = vessel.protoVessel.protoPartSnapshots[i].resources[j].resourceRef;

                    if(resource == null) continue;

                    var resourceInfo = new VesselResourceInfo
                    {
                        ResourceName = resource.resourceName,
                        PartPersistentId = vessel.protoVessel.protoPartSnapshots[i].persistentId,
                        PartFlightId = vessel.protoVessel.protoPartSnapshots[i].flightID,
                        Amount = resource.amount,
                        FlowState = resource.flowState
                    };
                    
                    Resources.Add(resourceInfo);
                }
            }
            
            msgData.Resources = Resources.ToArray();
            SendMessage(msgData);
        }
    }
}
