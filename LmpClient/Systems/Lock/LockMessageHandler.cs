using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Events;
using LmpCommon.Enums;
using LmpCommon.Locks;
using LmpCommon.Message.Data.Lock;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UniLinq;

namespace LmpClient.Systems.Lock
{
    public class LockMessageHandler : SubSystem<LockSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        private static readonly List<LockDefinition> LocksToRemove = new List<LockDefinition>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is LockBaseMsgData msgData)) return;

            switch (msgData.LockMessageType)
            {
                case LockMessageType.ListReply:
                    {
                        var data = (LockListReplyMsgData)msgData;

                        var currentLocks = LockSystem.LockQuery.GetAllLocks().ToList();
                        var missingLocks = data.Locks.Except(currentLocks);

                        foreach (var missingLock in missingLocks)
                        {
                            LockSystem.LockStore.AddOrUpdateLock(missingLock);
                            LockEvent.onLockAcquire.Fire(missingLock);
                        }

                        var corruptLocks = currentLocks.Except(data.Locks);
                        foreach (var corruptLock in corruptLocks)
                        {
                            LockSystem.LockStore.AddOrUpdateLock(corruptLock);
                            LockEvent.onLockAcquire.Fire(corruptLock);
                        }

                        if (MainSystem.NetworkState < ClientState.LocksSynced)
                            MainSystem.NetworkState = ClientState.LocksSynced;
                    }
                    break;
                case LockMessageType.Acquire:
                    {
                        var data = (LockAcquireMsgData)msgData;
                        LockSystem.LockStore.AddOrUpdateLock(data.Lock);

                        LockEvent.onLockAcquire.Fire(data.Lock);
                    }
                    break;
                case LockMessageType.Release:
                    {
                        var data = (LockReleaseMsgData)msgData;
                        LockSystem.LockStore.RemoveLock(data.Lock);

                        LockEvent.onLockRelease.Fire(data.Lock);
                    }
                    break;
            }
        }
    }
}
