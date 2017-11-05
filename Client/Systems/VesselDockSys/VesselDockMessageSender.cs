using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.VesselProtoSys;
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

        public void SendDockInformation(VesselDockStructure dock)
        {
            var vesselBytes = VesselSerializer.SerializeVessel(dock.DominantVessel.BackupVessel());
            if (vesselBytes.Length > 0)
            {
                SendMessage(new VesselDockMsgData
                {
                    WeakVesselId = dock.WeakVesselId,
                    DominantVesselId = dock.DominantVesselId,
                    FinalVesselData = vesselBytes
                });
            }
        }
    }
}
