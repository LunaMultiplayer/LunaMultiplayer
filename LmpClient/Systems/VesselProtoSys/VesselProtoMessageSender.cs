using Harmony;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;
using System.Collections;
using System.Reflection;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageSender : SubSystem<VesselProtoSystem>, IMessageSender
    {
        private static readonly FieldInfo OrbitDriverReady = typeof(OrbitDriver).GetField("ready", AccessTools.all);
        
        /// <summary>
        /// Pre allocated array to store the vessel data into it. Max 10 megabytes
        /// </summary>
        private static readonly byte[] VesselSerializedBytes = new byte[10 * 1024 * 1000];

        private static readonly object VesselArraySyncLock = new object();

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselMessage(Vessel vessel, bool forceReload = false)
        {
            if (vessel == null || vessel.state == Vessel.State.DEAD || VesselRemoveSystem.Singleton.VesselWillBeKilled(vessel.id))
                return;

            if (!vessel.orbitDriver)
            {
                LunaLog.LogWarning($"Cannot send vessel {vessel.vesselName} - {vessel.id}. It's orbit driver is null!");
                return;
            }

            if ((bool)OrbitDriverReady.GetValue(vessel.orbitDriver))
            {
                vessel.protoVessel = vessel.BackupVessel();
                SendVesselMessage(vessel.protoVessel);
            }
            else
            {
                //Orbit driver is not ready so wait 10 frames until it's ready
                CoroutineUtil.StartConditionRoutine("SendVesselMessage",
                    () => SendVesselMessage(vessel),
                    () => (bool)OrbitDriverReady.GetValue(vessel.orbitDriver), 10);
            }
        }

        #region Private methods

        private IEnumerator SendOnceOrbitDriverReady(Vessel vessel, int maxTries)
        {
            var tries = 0;

            while (!(bool)OrbitDriverReady.GetValue(vessel.orbitDriver) && tries < maxTries)
            {
                tries++;
                yield return 0;
            }

            vessel.protoVessel = vessel.BackupVessel();
            SendVesselMessage(vessel.protoVessel);
        }

        private void SendVesselMessage(ProtoVessel protoVessel)
        {
            if (protoVessel == null || protoVessel.vesselID == Guid.Empty) return;
            //Doing this in another thread can crash the game as during the serialization into a config node Lingoona is called...
            //TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel));
            PrepareAndSendProtoVessel(protoVessel);
        }

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel)
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
                    msgData.GameTime = TimeSyncSystem.UniversalTime;
                    msgData.VesselId = protoVessel.vesselID;
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
                        VesselRemoveSystem.Singleton.KillVessel(protoVessel.vesselID, true, "Serialization of debris failed");
                    }
                }
            }
        }

        #endregion
    }
}
