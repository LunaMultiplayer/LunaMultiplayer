using LmpCommon.Locks;
using LmpCommon.Message.Data.Lock;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using System.Linq;

namespace Server.System
{
    public class LockSystemSender
    {
        public static void SendAllLocks(ClientStructure client)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockListReplyMsgData>();
            msgData.Locks = LockSystem.LockQuery.GetAllLocks().ToArray();
            msgData.LocksCount = msgData.Locks.Length;

            MessageQueuer.SendToClient<LockSrvMsg>(client, msgData);
        }

        public static void ReleaseAndSendLockReleaseMessage(ClientStructure client, LockDefinition lockDefinition)
        {
            var lockReleaseResult = LockSystem.ReleaseLock(lockDefinition);
            if (lockReleaseResult)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockReleaseMsgData>();
                msgData.Lock = lockDefinition;
                msgData.LockResult = true;
                
                MessageQueuer.RelayMessage<LockSrvMsg>(client, msgData);
                LunaLog.Debug($"{lockDefinition.PlayerName} released lock {lockDefinition}");
            }
            else
            {
                SendStoredLockData(client, lockDefinition);
                LunaLog.Debug($"{lockDefinition.PlayerName} failed to release lock {lockDefinition}");
            }
        }

        public static void SendLockAquireMessage(ClientStructure client, LockDefinition lockDefinition, bool force)
        {
            if (LockSystem.AcquireLock(lockDefinition, force, out var repeatedAcquire))
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockAcquireMsgData>();
                msgData.Lock = lockDefinition;
                msgData.Force = force;

                MessageQueuer.SendToAllClients<LockSrvMsg>(msgData);

                //Just log it if we actually changed the value. Users might send repeated acquire locks as they take a bit of time to reach them...
                if (!repeatedAcquire) 
                    LunaLog.Debug($"{lockDefinition.PlayerName} acquired lock {lockDefinition}");
            }
            else
            {
                SendStoredLockData(client, lockDefinition);
                LunaLog.Debug($"{lockDefinition.PlayerName} failed to acquire lock {lockDefinition}");
            }
        }

        /// <summary>
        /// Whenever a release/acquire lock fails, call this method to relay the correct lock definition to the player
        /// </summary>
        private static void SendStoredLockData(ClientStructure client, LockDefinition lockDefinition)
        {
            var storedLockDef = LockSystem.LockQuery.GetLock(lockDefinition.Type, lockDefinition.PlayerName, lockDefinition.VesselId, lockDefinition.KerbalName);
            if (storedLockDef != null)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockAcquireMsgData>();
                msgData.Lock = storedLockDef;
                MessageQueuer.SendToClient<LockSrvMsg>(client, msgData);
            }
        }
    }
}
