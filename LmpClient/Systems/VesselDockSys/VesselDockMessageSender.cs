using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselDockSys
{
    public class VesselDockMessageSender : SubSystem<VesselDockSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg)));
        }

        public void SendDockInformation(Guid weakVesselId, Vessel dominantVessel, int subspaceId)
        {
            var vesselBytes = VesselSerializer.SerializeVessel(dominantVessel.BackupVessel());
            if (vesselBytes.Length > 0)
            {
                CreateAndSendDockMessage(weakVesselId, dominantVessel.id, subspaceId, vesselBytes);
            }
        }

        public void SendDockInformation(Guid weakVesselId, Vessel dominantVessel, int subspaceId, ProtoVessel finalDominantVesselProto)
        {
            if (finalDominantVesselProto != null)
            {
                var vesselBytes = VesselSerializer.SerializeVessel(finalDominantVesselProto);
                if (vesselBytes.Length > 0)
                {
                    CreateAndSendDockMessage(weakVesselId, dominantVessel.id, subspaceId, vesselBytes);
                }
            }
            else
            {
                SendDockInformation(weakVesselId, dominantVessel, subspaceId);
            }
        }

        private void CreateAndSendDockMessage(Guid weakVesselId, Guid dominantVesselId, int subspaceId, byte[] vesselBytes)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselDockMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.WeakVesselId = weakVesselId;
            msgData.DominantVesselId = dominantVesselId;

            msgData.FinalVesselData = vesselBytes;
            msgData.NumBytes = vesselBytes.Length;
            msgData.SubspaceId = subspaceId;

            SendMessage(msgData);
        }
    }
}
