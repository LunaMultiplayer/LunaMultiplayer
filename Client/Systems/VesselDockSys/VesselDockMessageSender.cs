using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Threading;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockMessageSender : SubSystem<VesselDockSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg)));
        }

        public void SendDockInformation(VesselDockStructure dock, int subspaceId, int delaySeconds = 0)
        {
            var vesselBytes = VesselSerializer.SerializeVessel(dock.DominantVessel.BackupVessel());
            if (vesselBytes.Length > 0)
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselDockMsgData>();
                msgData.WeakVesselId = dock.WeakVesselId;
                msgData.DominantVesselId = dock.DominantVesselId;
                msgData.FinalVesselData = vesselBytes;
                msgData.SubspaceId = subspaceId;

                TaskFactory.StartNew(() =>
                {
                    Thread.Sleep(delaySeconds);
                    NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msgData));
                });
            }
        }
    }
}
