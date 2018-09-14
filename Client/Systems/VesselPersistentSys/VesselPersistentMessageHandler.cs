using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPersistentSys
{
    public class VesselPersistentMessageHandler : SubSystem<VesselPersistentSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPersistentMsgData msgData)) return;

            if (!msgData.PartPersistentChange)
            {
                if (FlightGlobals.PersistentVesselIds.TryGetValue(msgData.From, out var existingVessel))
                {
                    existingVessel.persistentId = msgData.To;
                    existingVessel.protoVessel.persistentId = msgData.To;
                    FlightGlobals.PersistentVesselIds.Remove(msgData.From);
                    FlightGlobals.PersistentVesselIds.Add(msgData.To, existingVessel);

                    LockSystem.LockStore.UpdatePersistentId(msgData.From, msgData.To);
                }
            }
            else
            {
                if(FlightGlobals.PersistentLoadedPartIds.TryGetValue(msgData.From, out var existingLoadedPart))
                {
                    existingLoadedPart.persistentId = msgData.To;
                    existingLoadedPart.protoPartSnapshot.persistentId = msgData.To;

                    FlightGlobals.PersistentLoadedPartIds.Remove(msgData.From);
                    FlightGlobals.PersistentLoadedPartIds.Add(msgData.To, existingLoadedPart);
                }

                if (FlightGlobals.PersistentUnloadedPartIds.TryGetValue(msgData.From, out var existingUnloadedPart))
                {
                    existingUnloadedPart.persistentId = msgData.To;

                    FlightGlobals.PersistentUnloadedPartIds.Remove(msgData.From);
                    FlightGlobals.PersistentUnloadedPartIds.Add(msgData.To, existingUnloadedPart);
                }
            }
        }
    }
}
