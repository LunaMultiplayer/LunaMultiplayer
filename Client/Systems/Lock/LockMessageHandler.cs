using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Lock
{
    public class LockMessageHandler : SubSystem<LockSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is LockBaseMsgData msgData)) return;

            switch (msgData.LockMessageType)
            {
                case LockMessageType.ListReply:
                    {
                        var data = (LockListReplyMsgData)msgData;
                        LockSystem.LockStore.ClearAllLocks();

                        for (var i = 0; i < data.LocksCount; i++)
                        {
                            LockSystem.LockStore.AddOrUpdateLock(data.Locks[i]);
                        }

                        MainSystem.NetworkState = ClientState.LocksSynced;
                    }
                    break;
                case LockMessageType.Acquire:
                    {
                        var data = (LockAcquireMsgData)msgData;
                        LockSystem.LockStore.AddOrUpdateLock(data.Lock);

                        System.FireAcquireEvent(data.Lock);
                    }
                    break;
                case LockMessageType.Release:
                    {
                        var data = (LockReleaseMsgData)msgData;
                        LockSystem.LockStore.RemoveLock(data.Lock);

                        System.FireReleaseEvent(data.Lock);
                    }
                    break;
            }
        }
    }
}