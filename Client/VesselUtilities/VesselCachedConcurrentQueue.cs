using LunaClient.Base;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Interface;
using System;
using System.Threading;

namespace LunaClient.VesselUtilities
{
    public abstract class VesselCachedConcurrentQueue<T, TD> : CachedConcurrentQueue<T, TD> where T : new() where TD : IMessageData
    {
        private const int MaxPacketsInQueue = 5;
        private const float MaxTimeDifference = 1.5f;

        protected abstract bool CurrentDictionaryContainsKey(Guid vesselId);
        protected abstract int GetSubspaceIdFromValue(T value);
        protected abstract double GetTimestampFromValue(T value);

        protected VesselCachedConcurrentQueue(Guid vesselId) => SystemBase.LongRunTaskFactory.StartNew(() =>
        {
            while (CurrentDictionaryContainsKey(vesselId))
            {
                if (Queue.TryPeek(out var outValue))
                {
                    var subspaceId = GetSubspaceIdFromValue(outValue);

                    if (WarpSystem.Singleton.SubspaceIsEqualOrInThePast(subspaceId))
                    {
                        //We don't want to have more than 5 packets in the queue.
                        DequeueExtraPackets();
                    }
                    else if (WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(subspaceId) < MaxTimeDifference * 1.5f)
                    {
                        //We are in a subspace that even if it's in the future we have a time difference close to MaxTimeDifference
                        DequeueExtraPackets();
                    }
                    else if (TimeSyncerSystem.UniversalTime - GetTimestampFromValue(outValue) > MaxTimeDifference)
                    {
                        //We are in a subspace that it's in the future. Let's remove the messages that are too old...
                        while (Queue.TryDequeue(out var dequeuedVal) && TimeSyncerSystem.UniversalTime - GetTimestampFromValue(dequeuedVal) > MaxTimeDifference)
                        {
                            Recycle(outValue);
                        }
                    }
                }
                Thread.Sleep(5);
            }
        });

        private void DequeueExtraPackets()
        {
            while (Count > MaxPacketsInQueue && Queue.TryDequeue(out var outExceededValue))
            {
                Recycle(outExceededValue);
            }
        }
    }
}
