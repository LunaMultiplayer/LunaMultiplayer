using LunaCommon.Locks;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using System.Linq;

namespace LunaServer.System
{
    public class LockSystemSender
    {
        public static void SendAllLocks(ClientStructure client)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockListReplyMsgData>();
            msgData.Locks = LockSystem.LockQuery.GetAllLocks().ToArray();

            MessageQueuer.SendToClient<LockSrvMsg>(client, msgData);
        }

        public static void ReleaseAndSendLockReleaseMessage(LockDefinition lockDefinition)
        {
            var lockResult = LockSystem.ReleaseLock(lockDefinition);
            if (lockResult)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockReleaseMsgData>();
                msgData.Lock = lockDefinition;
                msgData.LockResult = true;
                
                MessageQueuer.SendToAllClients<LockSrvMsg>(msgData);
                LunaLog.Debug($"{lockDefinition.PlayerName} released lock {lockDefinition}");
            }
            else
            {
                LunaLog.Debug($"{lockDefinition.PlayerName} failed to release lock {lockDefinition}");
            }
        }

        public static void SendLockAquireMessage(LockDefinition lockDefinition, bool force)
        {
            var lockResult = LockSystem.AcquireLock(lockDefinition, force);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockAcquireMsgData>();
            msgData.Lock = lockDefinition;
            msgData.LockResult = lockResult;
            msgData.Force = force;
            
            MessageQueuer.SendToAllClients<LockSrvMsg>(msgData);

            LunaLog.Debug(lockResult
                ? $"{lockDefinition.PlayerName} acquired lock {lockDefinition}"
                : $"{lockDefinition.PlayerName} failed to acquire lock {lockDefinition}");
        }
    }
}