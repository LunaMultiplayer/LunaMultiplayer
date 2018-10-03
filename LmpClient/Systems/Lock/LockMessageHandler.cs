using System.Collections.Concurrent;
using System.Collections.Generic;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Events;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;
using LmpCommon.Locks;
using LmpCommon.Message.Data.Lock;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
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

                        for (var i = 0; i < data.LocksCount; i++)
                        {
                            LockSystem.LockStore.AddOrUpdateLock(data.Locks[i]);
                        }

                        RemoveCorruptLocks(data);

                        if (MainSystem.NetworkState < ClientState.LocksSynced)
                            MainSystem.NetworkState = ClientState.LocksSynced;
                    }
                    break;
                case LockMessageType.Acquire:
                    {
                        var data = (LockAcquireMsgData)msgData;
                        LockSystem.LockStore.AddOrUpdateLock(data.Lock);

                        LockEvent.onLockAcquire.Fire(data.Lock);
                        System.AcquiredLocks.Enqueue(data.Lock);
                    }
                    break;
                case LockMessageType.Release:
                    {
                        var data = (LockReleaseMsgData)msgData;
                        LockSystem.LockStore.RemoveLock(data.Lock);

                        LockEvent.onLockRelease.Fire(data.Lock);
                        System.ReleasedLocks.Enqueue(data.Lock);
                    }
                    break;
            }
        }

        /// <summary>
        /// Here we run trough OUR lock list and if we find a lock that is not ours and it does not exist on the server, we remove it.
        /// We don't remove OUR locks as perhaps we just acquired one and we are waiting for the server to receive it.
        /// </summary>
        private static void RemoveCorruptLocks(LockListReplyMsgData data)
        {
            LocksToRemove.Clear();
            foreach (var existingLock in LockSystem.LockQuery.GetAllLocks())
            {
                if (existingLock.PlayerName != SettingsSystem.CurrentSettings.PlayerName && !data.Locks.Contains(existingLock))
                    LocksToRemove.Add(existingLock);
            }
            foreach (var lockToRemove in LocksToRemove)
            {
                LockSystem.LockStore.RemoveLock(lockToRemove);
            }
        }
    }
}
