using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Generic;


namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageSender : SubSystem<VesselProtoSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(Vessel vessel)
        {
            if (vessel == null || vessel.situation == Vessel.Situations.PRELAUNCH || VesselCommon.IsInSafetyBubble(vessel))
                return;

            /* Doing a Vessel.Backup takes a lot of time... 
             * I tried to make a handler that redo the constructor of ProtoVessel but 
             * it's very unstable as lingoona acces invalid addresses...
             * Looks like we won't be able to backup the proto in another thread, at least
             * in the near future :(
             * Do not call it in this way as we will send a NOT UPDATED protovessel!
             * SendVesselMessage(vessel.protoVessel);
             */
            var backup = vessel.BackupVessel();
            SendVesselMessage(backup);
        }
        
        public void SendVesselMessage(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return;
            TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel));
        }

        public void SendVesselMessage(IEnumerable<Vessel> vessels)
        {
            foreach (var vessel in vessels)
            {
                SendVesselMessage(vessel);
            }
        }

        #region Private methods

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel)
        {
            var vesselBytes = VesselSerializer.SerializeVessel(protoVessel);
            if (vesselBytes.Length > 0)
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoMsgData>();
                msgData.VesselId = protoVessel.vesselID;
                msgData.VesselData = vesselBytes;

                SendMessage(msgData);
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to create byte[] data for {protoVessel.vesselID}");
            }
        }

        #endregion
    }
}
