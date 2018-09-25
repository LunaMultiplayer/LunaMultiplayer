using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageSender : SubSystem<VesselRemoveSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg))); ;
        }

        /// <summary>
        /// Sends a vessel remove to the server. If keepVesselInRemoveList is set to true, the vessel will be removed for good and the server
        /// will skip future updates related to this vessel
        /// </summary>
        public void SendVesselRemove(Vessel vessel, bool keepVesselInRemoveList = true)
        {
            if (vessel == null) return;

            SendVesselRemove(vessel.id, vessel.persistentId, keepVesselInRemoveList);
        }

        /// <summary>
        /// Sends a vessel remove to the server. If keepVesselInRemoveList is set to true, the vessel will be removed for good and the server
        /// will skip future updates related to this vessel
        /// </summary>
        public void SendVesselRemove(Guid vesselId, uint vesselPersistentId, bool keepVesselInRemoveList = true)
        {
            LunaLog.Log($"[LMP]: Removing {vesselId}-{vesselPersistentId} from the server");
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselRemoveMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.VesselPersistentId = vesselPersistentId;
            msgData.AddToKillList = keepVesselInRemoveList;

            SendMessage(msgData);
        }
    }
}
