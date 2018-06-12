using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Generic;


namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageSender : SubSystem<VesselProtoSystem>, IMessageSender
    {
        /// <summary>
        /// Pre allocated array to store the vessel data into it. Max 10 megabytes
        /// </summary>
        private static readonly byte[] VesselSerializedBytes = new byte[10 * 1024 * 1000];

        private static readonly object VesselArraySyncLock = new object();

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(IEnumerable<Vessel> vessels)
        {
            foreach (var vessel in vessels)
            {
                SendVesselMessage(vessel, false, false);
            }
        }

        public void SendVesselMessage(Vessel vessel, bool forceSend, bool forceReloadOnReceive)
        {
            if (vessel == null || (!forceSend && VesselCommon.IsSpectating) || vessel.state == Vessel.State.DEAD || VesselRemoveSystem.Singleton.VesselWillBeKilled(vessel.id))
                return;

            if (!forceSend && !LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                return;
            if (!forceSend && !LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                return;

            var vesselHasChanges = VesselToProtoRefresh.RefreshVesselProto(vessel);

            if (forceSend || vesselHasChanges || !VesselsProtoStore.AllPlayerVessels.ContainsKey(vessel.id))
                SendVesselMessage(vessel.BackupVessel(), forceReloadOnReceive);

            if (!VesselsProtoStore.AllPlayerVessels.ContainsKey(vessel.id))
                VesselsProtoStore.AddOrUpdateVesselToDictionary(vessel);
        }

        #region Private methods

        private void SendVesselMessage(ProtoVessel protoVessel, bool forceReloadOnReceive)
        {
            if (protoVessel == null || protoVessel.vesselID == Guid.Empty) return;
            //Doing this in another thread can crash the game as during the serialization into a config node Lingoona is called...
            //TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel));
            PrepareAndSendProtoVessel(protoVessel, forceReloadOnReceive);
        }

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel, bool forceReloadOnReceive)
        {
            //Never send empty vessel id's (it happens with flags...)
            if (protoVessel.vesselID == Guid.Empty || protoVessel.vesselName == null) return;

            //VesselSerializedBytes is shared so lock it!
            lock (VesselArraySyncLock)
            {
                VesselSerializer.SerializeVesselToArray(protoVessel, VesselSerializedBytes, out var numBytes);
                if (numBytes > 0)
                {
                    VesselsProtoStore.RawUpdateVesselProtoData(VesselSerializedBytes, numBytes, protoVessel.vesselID);

                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoMsgData>();
                    msgData.GameTime = TimeSyncerSystem.UniversalTime;
                    FillAndSendProtoMessageData(protoVessel.vesselID, msgData, VesselSerializedBytes, numBytes);
                    msgData.ForceReload = forceReloadOnReceive;
                }
                else
                {
                    if (protoVessel.vesselType == VesselType.Debris)
                    {
                        LunaLog.Log($"Serialization of debris vessel: {protoVessel.vesselID} name: {protoVessel.vesselName} failed. Adding to kill list");
                        VesselRemoveSystem.Singleton.AddToKillList(protoVessel.vesselID, "Serialization of debris failed");
                    }
                }
            }
        }

        private void FillAndSendProtoMessageData(Guid vesselId, VesselProtoMsgData msgData, byte[] vesselBytes, int numBytes)
        {
            msgData.VesselId = vesselId;

            if (msgData.Data.Length < numBytes)
                Array.Resize(ref msgData.Data, numBytes);

            Array.Copy(vesselBytes, 0, msgData.Data, 0, numBytes);
            msgData.NumBytes = numBytes;

            SendMessage(msgData);
        }

        #endregion
    }
}
