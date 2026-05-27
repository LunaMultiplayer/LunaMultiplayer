using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselProtoSys
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

        public void SendVesselMessage(Vessel vessel, bool forceReload = false)
        {
            if (vessel == null || vessel.state == Vessel.State.DEAD || VesselRemoveSystem.Singleton.VesselWillBeKilled(vessel.id))
                return;

            if (!vessel.orbitDriver)
            {
                LunaLog.LogWarning($"Cannot send vessel {vessel.vesselName} - {vessel.id}. It's orbit driver is null!");
                return;
            }

            if (vessel.orbitDriver.Ready())
            {
                vessel.protoVessel = vessel.BackupVessel();
                SendVesselMessage(vessel.protoVessel, forceReload);
            }
            else
            {
                //Orbit driver is not ready so wait max 10 frames until it's ready
                CoroutineUtil.StartConditionRoutine("SendVesselMessage",
                    () => SendVesselMessage(vessel),
                    () => vessel.orbitDriver.Ready(), 10);
            }
        }

        #region Private methods

        private void SendVesselMessage(ProtoVessel protoVessel, bool forceReload)
        {
            if (protoVessel == null || protoVessel.vesselID == Guid.Empty) return;
            // ConfigNode serialization calls into Lingoona which can be thread-sensitive.
            // PrepareAndSendProtoVessel now has a full try/catch so failures are logged, not fatal.
            TaskFactory.StartNew(() => PrepareAndSendProtoVessel(protoVessel, forceReload));
        }

        /// <summary>
        /// This method prepares the protovessel class and send the message, it's intended to be run in another thread.
        /// The entire body is wrapped in a try/catch because ConfigNode serialization via Lingoona can fail in some
        /// Unity versions and we must not propagate the exception onto the background thread pool.
        /// </summary>
        private void PrepareAndSendProtoVessel(ProtoVessel protoVessel, bool forceReload)
        {
            if (protoVessel.vesselID == Guid.Empty) return;

            try
            {
                lock (VesselArraySyncLock)
                {
                    VesselSerializer.SerializeVesselToArray(protoVessel, VesselSerializedBytes, out var numBytes);
                    if (numBytes > 0)
                    {
                        var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselProtoMsgData>();
                        msgData.GameTime = TimeSyncSystem.UniversalTime;
                        msgData.VesselId = protoVessel.vesselID;
                        msgData.NumBytes = numBytes;
                        msgData.ForceReload = forceReload;
                        if (msgData.Data.Length < numBytes)
                            Array.Resize(ref msgData.Data, numBytes);
                        Array.Copy(VesselSerializedBytes, 0, msgData.Data, 0, numBytes);

                        SendMessage(msgData);
                    }
                    else
                    {
                        if (protoVessel.vesselType == VesselType.Debris)
                        {
                            LunaLog.Log($"Serialization of debris vessel {protoVessel.vesselID} ({protoVessel.vesselName}) failed — adding to kill list");
                            VesselRemoveSystem.Singleton.KillVessel(protoVessel.vesselID, true, "Serialization of debris failed");
                        }
                        else
                        {
                            LunaLog.LogWarning($"Serialization of vessel {protoVessel.vesselID} ({protoVessel.vesselName}) produced 0 bytes — skipping send");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LunaLog.LogError($"[LMP]: Exception serializing vessel {protoVessel.vesselID} ({protoVessel.vesselName}): {ex}");
            }
        }

        #endregion
    }
}
