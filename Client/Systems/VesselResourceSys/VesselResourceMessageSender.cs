using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselResourceSys
{
    public class VesselResourceMessageSender : SubSystem<VesselResourceSystem>, IMessageSender
    {
        private static VesselResourceInfo[] _resources = new VesselResourceInfo[0];

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselResources(Vessel vessel)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselResourceMsgData>();
            msgData.VesselId = vessel.id;

            var resourcesCount = GetResourcesCount(vessel);
            if (resourcesCount == 0) return; //No need to send msg if there's no resources in the vessel

            msgData.ResourcesCount = resourcesCount;

            if (_resources.Length < resourcesCount)
                _resources = new VesselResourceInfo[resourcesCount];

            var resourceIndex = 0;
            for (var i = 0; i < vessel.Parts.Count; i++)
            {
                for (var j = 0; j < vessel.Parts[i].Resources.Count; j++)
                {
                    if (_resources[resourceIndex] == null)
                        _resources[resourceIndex] = new VesselResourceInfo();

                    _resources[resourceIndex].ResourceName = vessel.Parts[i].Resources[j].resourceName;
                    _resources[resourceIndex].PartFlightId = vessel.Parts[i].flightID;
                    _resources[resourceIndex].Amount = vessel.Parts[i].Resources[j].amount;

                    resourceIndex++;
                }
            }

            if (msgData.Resources.Length < resourcesCount)
                msgData.Resources = new VesselResourceInfo[resourcesCount];

            Array.Copy(_resources, msgData.Resources, resourcesCount);

            //Update our own values in the store aswell as otherwise if we leave the vessel it can still be in the safety bubble
            VesselsProtoStore.UpdateVesselProtoResources(msgData);

            SendMessage(msgData);
        }

        /// <summary>
        /// Returns the number of resource in all the parts of the vessel without generating garbage
        /// </summary>
        private static int GetResourcesCount(Vessel vessel)
        {
            var count = 0;
            for (var i = 0; i < vessel.Parts.Count; i++)
            {
                count += vessel.Parts[i].Resources.Count;
            }

            return count;
        }
    }
}
