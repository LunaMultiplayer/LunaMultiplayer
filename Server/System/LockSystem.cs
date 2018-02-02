using LunaCommon.Locks;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System
{
    public class LockSystem
    {
        private static readonly LockStore LockStore = new LockStore();
        public static readonly LockQuery LockQuery = new LockQuery(LockStore);
        
        public static bool AcquireLock(LockDefinition lockDef, bool force, out bool repeatedAcquire)
        {
            if (force || !LockQuery.LockExists(lockDef))
            {
                repeatedAcquire = LockQuery.LockBelongsToPlayer(lockDef.Type, lockDef.VesselId, lockDef.PlayerName);
                LockStore.AddOrUpdateLock(lockDef);
                return true;
            }
            repeatedAcquire = false;
            return false;
        }

        public static bool ReleaseLock(LockDefinition lockDef)
        {
            if (LockQuery.LockExists(lockDef) && LockQuery.GetLock(lockDef.Type, lockDef.PlayerName, lockDef.VesselId)
                .PlayerName == lockDef.PlayerName)
            {
                LockStore.RemoveLock(lockDef);
                return true;
            }
            return false;
        }

        public static void ReleasePlayerLocks(ClientStructure client)
        {
            var removeList = LockQuery.GetAllPlayerLocks(client.PlayerName);

            foreach (var lockToRemove in removeList)
            {
                if (lockToRemove.Type == LockType.Control && !GeneralSettings.SettingsStore.DropControlOnExit)
                    continue;

                LockSystemSender.ReleaseAndSendLockReleaseMessage(client, lockToRemove);
            }
        }

        /// <summary>
        /// This method send all locks to all clients at a interval of 30 seconds.
        /// </summary>
        public static async void SendAllLocks()
        {
            while (ServerContext.ServerRunning)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<LockListReplyMsgData>();
                msgData.Locks = LockQuery.GetAllLocks().ToArray();
                msgData.LocksCount = msgData.Locks.Length;

                MessageQueuer.SendToAllClients<LockSrvMsg>(msgData);

                await Task.Delay(30000);
            }
        }
    }
}