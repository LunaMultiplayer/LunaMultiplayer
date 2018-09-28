using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;
using UniLinq;

namespace LmpClient.Systems.VesselSyncSys
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
            msgData.GameTime = TimeSyncerSystem.UniversalTime;

            var vesselIds = FlightGlobals.Vessels.Where(v=> v!= null).Select(v => v.id).ToArray();
            msgData.VesselsCount = vesselIds.Length;

            //Always clear the array just for safety...
            for (var i = 0; i < msgData.VesselIds.Length; i++)
            {
                msgData.VesselIds[i] = Guid.Empty;
            }

            if (msgData.VesselIds.Length < msgData.VesselsCount)
                msgData.VesselIds = new Guid[msgData.VesselsCount];

            Array.Copy(vesselIds, msgData.VesselIds, msgData.VesselsCount);

            SendMessage(msgData);
        }
    }
}
