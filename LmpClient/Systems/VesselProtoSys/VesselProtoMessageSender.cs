using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSyncer;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;
using System.Collections.Generic;


namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageSender : SubSystem<VesselProtoSystem>, IMessageSender
    {
        /// <summary>
        /// Pre allocated array to store the vessel data into it. Max 10 megabytes
        /// </summary>
        private static readonly byte[] VesselSerializedBytes = new byte[10 * 1024 * 1000];

        private static readonly object VesselArraySyncLock = new object();

        private static Dictionary<Guid, Vessel> VesselsWithPartsChanged = new Dictionary<Guid, Vessel>();

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(IEnumerable<Vessel> vessels)
        {
            foreach (var vessel in vessels)
            {
                SendVesselMessage(vessel, false);
            }
        }

        public void SendVesselMessage(Vessel vessel, bool forceReloadOnReceive)
        {
            if (vessel == null || vessel.state == Vessel.State.DEAD || VesselRemoveSystem.Singleton.VesselWillBeKilled(vessel.id))
                return;

            vessel.protoVessel = vessel.BackupVessel();
            SendVesselMessage(vessel.protoVessel, forceReloadOnReceive);
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
            if (protoVessel.vesselID == Guid.Empty) return;

            //VesselSerializedBytes is shared so lock it!
            lock (VesselArraySyncLock)
            {
                VesselSerializer.SerializeVesselToArray(protoVessel, VesselSerializedBytes, out var numBytes);
                if (numBytes > 0)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoMsgData>();
                    msgData.GameTime = TimeSyncerSystem.UniversalTime;
                    msgData.ForceReload = forceReloadOnReceive;
                    msgData.VesselId = protoVessel.vesselID;
                    msgData.VesselPersistentId = protoVessel.persistentId;
                    msgData.NumBytes = numBytes;
                    if (msgData.Data.Length < numBytes)
                        Array.Resize(ref msgData.Data, numBytes);
                    Array.Copy(VesselSerializedBytes, 0, msgData.Data, 0, numBytes);

                    SendMessage(msgData);
                }
                else
                {
                    if (protoVessel.vesselType == VesselType.Debris)
                    {
                        LunaLog.Log($"Serialization of debris vessel: {protoVessel.vesselID} name: {protoVessel.vesselName} failed. Adding to kill list");
                        VesselRemoveSystem.Singleton.AddToKillList(protoVessel, "Serialization of debris failed");
                    }
                }
            }
        }

        #endregion
    }
}
