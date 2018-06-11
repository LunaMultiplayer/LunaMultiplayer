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
        public void SendVesselRemove(Guid vesselId, bool keepVesselInRemoveList = true, bool forceServerSideRemove = false, bool fastKill = false)
        {
            LunaLog.Log($"[LMP]: Removing {vesselId} from the server");
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselRemoveMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vesselId;
            msgData.AddToKillList = keepVesselInRemoveList;
            msgData.FastKill = keepVesselInRemoveList || fastKill;
            msgData.Force = forceServerSideRemove;

            SendMessage(msgData);
        }
    }
}
