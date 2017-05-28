using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Log;
using LunaServer.Server;
using System.Linq;

namespace LunaServer.System
{
    public class LockSystemSender
    {
        public static void SendAllLocks(ClientStructure client)
        {
            MessageQueuer.SendToClient<LockSrvMsg>(client, new LockListReplyMsgData { Locks = LockSystem.GetLockList().ToArray() });
        }

        public static void SendLockReleaseMessage(string lockName, string playerName)
        {
            var lockResult = LockSystem.ReleaseLock(lockName, playerName);
            if (lockResult)
            {
                var newMessageData = new LockReleaseMsgData
                {
                    PlayerName = playerName,
                    LockName = lockName,
                    LockResult = true
                };
                MessageQueuer.SendToAllClients<LockSrvMsg>(newMessageData);
                LunaLog.Debug($"{playerName} released lock {lockName}");
            }
            else
            {
                LunaLog.Debug($"{playerName} failed to release lock {lockName}");
            }
        }

        public static void SendLockAquireMessage(string lockName, string playerName, bool force)
        {
            var lockResult = LockSystem.AcquireLock(lockName, playerName, force);

            var newMessageData = new LockAcquireMsgData
            {
                PlayerName = playerName,
                LockName = lockName,
                LockResult = lockResult,
                Force = force
            };
            MessageQueuer.SendToAllClients<LockSrvMsg>(newMessageData);

            LunaLog.Debug(lockResult
                ? $"{playerName} acquired lock {lockName}"
                : $"{playerName} failed to acquire lock {lockName}");
        }
    }
}