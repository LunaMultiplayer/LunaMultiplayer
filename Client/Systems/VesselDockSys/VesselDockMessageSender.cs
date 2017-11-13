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
        private int _delaySeconds;

        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() =>
            {
                if (_delaySeconds > 0)
                    Thread.Sleep(_delaySeconds);
                NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
            });
        }

        public void SendDockInformation(VesselDockStructure dock, int delaySeconds = 0)
        {
            _delaySeconds = delaySeconds;
            var vesselBytes = VesselSerializer.SerializeVessel(dock.DominantVessel.BackupVessel());
            if (vesselBytes.Length > 0)
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselDockMsgData>();
                msgData.WeakVesselId = dock.WeakVesselId;
                msgData.DominantVesselId = dock.DominantVesselId;
                msgData.FinalVesselData = vesselBytes;

                SendMessage(msgData);
            }
        }
    }
}
