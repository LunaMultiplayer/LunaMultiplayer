using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselWarpSys;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

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
                case LockMessageType.LIST_REPLY:
                    {
                        var data = (LockListReplyMsgData)messageData;
                        System.ServerLocks.Clear();

                        foreach (var lockKey in data.Locks)
                        {
                            System.ServerLocks.Add(lockKey.Key, lockKey.Value);
                        }

                        VesselWarpSystem.Singleton.SyncVesselLocksAndSubspaces();
                        MainSystem.Singleton.NetworkState = ClientState.LOCKS_SYNCED;
                    }
                    break;
                case LockMessageType.ACQUIRE:
                    {
                        var data = (LockAcquireMsgData)messageData;

                        if (data.LockResult)
                            System.ServerLocks[data.LockName] = data.PlayerName;

                        System.FireAcquireEvent(data.PlayerName, data.LockName, data.LockResult);
                    }
                    break;
                case LockMessageType.RELEASE:
                    {
                        var data = (LockReleaseMsgData)messageData;
                        if (System.ServerLocks.ContainsKey(data.LockName))
                            System.ServerLocks.Remove(data.LockName);

                        System.FireReleaseEvent(data.PlayerName, data.LockName);
                    }
                    break;
            }
        }
    }
}