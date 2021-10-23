using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
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
            var resourceCount = 0;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselResourceMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;

            for (var i = 0; i < vessel.protoVessel.protoPartSnapshots.Count; i++)
            {
                if (vessel.protoVessel.protoPartSnapshots[i]?.resources == null) continue;

                for (var j = 0; j < vessel.protoVessel.protoPartSnapshots[i].resources.Count; j++)
                {
                    var resource = vessel.protoVessel.protoPartSnapshots[i].resources[j]?.resourceRef;
                    if (resource == null) continue;

                    if (Resources.Count > resourceCount)
                    {
                        Resources[resourceCount].ResourceName = resource.resourceName;
                        Resources[resourceCount].PartFlightId = vessel.protoVessel.protoPartSnapshots[i].flightID;
                        Resources[resourceCount].Amount = resource.amount;
                        Resources[resourceCount].FlowState = resource.flowState;
                    }
                    else
                    {
                        Resources.Add(new VesselResourceInfo
                        {
                            ResourceName = resource.resourceName,
                            PartFlightId = vessel.protoVessel.protoPartSnapshots[i].flightID,
                            Amount = resource.amount,
                            FlowState = resource.flowState
                        });
                    }

                    resourceCount++;
                }
            }

            msgData.ResourcesCount = resourceCount;

            if (msgData.Resources.Length < resourceCount)
            {
                msgData.Resources = new VesselResourceInfo[resourceCount];
            }

            for (var i = 0; i < resourceCount; i++)
            {
                if (msgData.Resources[i] == null)
                    msgData.Resources[i] = new VesselResourceInfo(Resources[i]);
                else
                    msgData.Resources[i].CopyFrom(Resources[i]);
            }

            SendMessage(msgData);
        }
    }
}
