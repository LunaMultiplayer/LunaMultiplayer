using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using UniLinq;

namespace LunaClient.Systems.VesselSyncSys
{
    public class VesselSyncMessageSender : SubSystem<VesselSyncSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }
        
        public void SendVesselsSyncMsg()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselSyncMsgData>();

            var vesselIds = VesselsProtoStore.AllPlayerVessels.Keys.ToArray();
            msgData.VesselsCount = vesselIds.Length;

            //Always clear the array just for safety...
            for (var i = 0; i < msgData.VesselIds.Length; i++)
            {
                msgData.VesselIds[i] = Guid.Empty;
            }

            if (msgData.VesselIds.Length < msgData.VesselsCount)
                Array.Resize(ref msgData.VesselIds, msgData.VesselsCount);

            Array.Copy(vesselIds, msgData.VesselIds, msgData.VesselsCount);

            SendMessage(msgData);
        }
    }
}
