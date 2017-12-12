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
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(Vessel vessel, bool reliable)
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
            SendVesselMessage(backup, reliable);
        }
        
        public void SendVesselMessage(ProtoVessel protoVessel, bool reliable)
        {
            if (protoVessel == null) return;
            TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel, reliable));
        }

        public void SendVesselMessage(IEnumerable<Vessel> vessels, bool reliable)
        {
            foreach (var vessel in vessels)
            {
                SendVesselMessage(vessel, reliable);
            }
        }

        #region Private methods

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel, bool reliable)
        {
            //FIX THIS
            reliable = true;
            //Never send empty vessel id's (it happens with flags...)
            if (protoVessel.vesselID == Guid.Empty) return;

            var vesselBytes = VesselSerializer.SerializeVessel(protoVessel);
            if (vesselBytes.Length > 0)
            {
                if (reliable)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoReliableMsgData>();
                    FillAndSendProtoMessageData(protoVessel.vesselID, msgData, vesselBytes);
                }
                else
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoMsgData>();
                    FillAndSendProtoMessageData(protoVessel.vesselID, msgData, vesselBytes);
                }
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to create byte[] data for {protoVessel.vesselID}");
            }
        }

        private void FillAndSendProtoMessageData(Guid vesselId, VesselProtoBaseMsgData msgData, byte[] vesselBytes)
        {
            msgData.VesselId = vesselId;
            msgData.VesselData = vesselBytes;

            SendMessage(msgData);
        }

        #endregion
    }
}
