using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;


namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageSender : SubSystem<VesselProtoSystem>, IMessageSender
    {
        /// <summary>
        /// Pre allocated array to store the vessel data into it. Max 10 megabytes
        /// </summary>
        private static readonly byte[] VesselSerializedBytes = new byte[10 * 1024 * 1000];

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(IEnumerable<Vessel> vessels)
        {
            foreach (var vessel in vessels)
            {
                SendVesselMessage(vessel, false);
            }
        }

        public void SendVesselMessage(Vessel vessel, bool force)
        {
            if (vessel == null || VesselCommon.IsSpectating)
                return;

            var vesselHasChanges = VesselProtoRefresh.RefreshVesselProto(vessel);

            //TODO: Now we send the protovessel all the time if someone is spectating us, perhaps we can just make a new system that sends the resources
            //as this will be better in terms of memory garbage...
            if (force || vesselHasChanges || VesselCommon.IsSomeoneSpectatingUs)
                SendVesselMessage(vessel.protoVessel);
        }

        #region Private methods

        private void SendVesselMessage(ProtoVessel protoVessel)
        {
            if (protoVessel == null || protoVessel.vesselID == Guid.Empty) return;
            TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel));
        }

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel)
        {
            //Never send empty vessel id's (it happens with flags...)
            if (protoVessel.vesselID == Guid.Empty || protoVessel.vesselName == null) return;

            //VesselSerializedBytes is shared so lock it!
            lock (VesselSerializedBytes)
            {
                VesselSerializer.SerializeVesselToArray(protoVessel, VesselSerializedBytes, out var numBytes);
                if (numBytes > 0)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoMsgData>();
                    msgData.Vessel.VesselSituation = (int) protoVessel.situation;
                    FillAndSendProtoMessageData(protoVessel.vesselID, msgData, VesselSerializedBytes, numBytes);
                }
            }
        }

        private void FillAndSendProtoMessageData(Guid vesselId, VesselProtoMsgData msgData, byte[] vesselBytes, int numBytes)
        {
            msgData.Vessel.VesselId = vesselId;

            if (msgData.Vessel.Data.Length < numBytes)
                Array.Resize(ref msgData.Vessel.Data, numBytes);

            Array.Copy(vesselBytes, 0, msgData.Vessel.Data, 0, numBytes);
            msgData.Vessel.NumBytes = numBytes;
            
            SendMessage(msgData);
        }

        #endregion
    }
}
