using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockMessageSender : SubSystem<VesselDockSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg)));
        }

        public void SendDockInformation(VesselDockStructure dock, int subspaceId)
        {
            var vesselBytes = VesselSerializer.SerializeVessel(dock.DominantVessel.BackupVessel());
            if (vesselBytes.Length > 0)
            {
                CreateAndSendDockMessage(dock, subspaceId, vesselBytes);
            }
        }

        public void SendDockInformation(VesselDockStructure dock, int subspaceId, ProtoVessel finalDominantVesselProto)
        {
            if (finalDominantVesselProto != null)
            {
                var vesselBytes = VesselSerializer.SerializeVessel(finalDominantVesselProto);
                if (vesselBytes.Length > 0)
                {
                    CreateAndSendDockMessage(dock, subspaceId, vesselBytes);
                }
            }
            else
            {
                SendDockInformation(dock, subspaceId);
            }
        }

        private void CreateAndSendDockMessage(VesselDockStructure dock, int subspaceId, byte[] vesselBytes)
        {
            //TODO: When would dock be null?
            if (dock == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselDockMsgData>();
            msgData.GameTime = Planetarium.GetUniversalTime();
            msgData.WeakVesselId = dock.WeakVesselId;
            msgData.DominantVesselId = dock.DominantVesselId;
            msgData.FinalVesselData = vesselBytes;
            msgData.NumBytes = vesselBytes.Length;
            msgData.SubspaceId = subspaceId;

            SendMessage(msgData);
        }
    }
}
