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
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as LockBaseMsgData;
            if (msgData == null) return;

            switch (msgData.LockMessageType)
            {
                case LockMessageType.ListReply:
                    {
                        var data = (LockListReplyMsgData)messageData;
                        LockSystem.LockStore.ClearAllLocks();

                        foreach (var lockKey in data.Locks)
                        {
                            LockSystem.LockStore.AddOrUpdateLock(lockKey);
                        }

                        MainSystem.NetworkState = ClientState.LocksSynced;
                    }
                    break;
                case LockMessageType.Acquire:
                    {
                        var data = (LockAcquireMsgData)messageData;

                        if (data.LockResult)
                            LockSystem.LockStore.AddOrUpdateLock(data.Lock);

                        System.FireAcquireEvent(data.Lock, data.LockResult);
                    }
                    break;
                case LockMessageType.Release:
                    {
                        var data = (LockReleaseMsgData)messageData;
                        LockSystem.LockStore.RemoveLock(data.Lock);

                        System.FireReleaseEvent(data.Lock);
                    }
                    break;
            }
        }
    }
}