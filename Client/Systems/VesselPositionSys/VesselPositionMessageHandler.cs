using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return;
            
            var vesselId = msgData.VesselId;
            if (!VesselCommon.DoVesselChecks(vesselId))
                return;

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoPosition(msgData);

            var update = CreatePosUpdateFromMessage(msgData);

            if (!VesselPositionSystem.CurrentVesselUpdate.ContainsKey(vesselId))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, update);
                VesselPositionSystem.TargetVesselUpdateQueue.TryAdd(vesselId, new FixedSizedConcurrentQueue<VesselPositionUpdate>(VesselPositionSystem.MaxQueuedUpdates));
            }
            else
            {
                if (VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(vesselId, out var queue))
                {
                    queue.Enqueue(update);
                }
            }
        }

        private static VesselPositionUpdate CreatePosUpdateFromMessage(VesselPositionMsgData msgData)
        {
            var update = new VesselPositionUpdate
            {
                VesselId = msgData.VesselId,
                BodyIndex = msgData.BodyIndex,
                HeightFromTerrain = msgData.HeightFromTerrain,
                GameTimeStamp = msgData.GameTime,
                ReceiveTime = LunaTime.UtcNow
            };

            Array.Copy(msgData.SrfRelRotation, update.SrfRelRotation, 4);
            Array.Copy(msgData.Velocity, update.Velocity, 3);
            Array.Copy(msgData.LatLonAlt, update.LatLonAlt, 3);
            Array.Copy(msgData.NormalVector, update.NormalVector, 3);
            Array.Copy(msgData.Orbit, update.Orbit, 8);

            return update;
        }
    }
}
