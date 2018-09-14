using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselRemoveSys
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
            LunaLog.Log($"[LMP]: Removing {vessel.id}-{vessel.persistentId} from the server");
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselRemoveMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.VesselPersistentId = vessel.persistentId;
            msgData.AddToKillList = keepVesselInRemoveList;

            SendMessage(msgData);
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
