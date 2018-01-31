using LunaCommon.Locks;
using Server.Client;
using Server.Settings;

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
    }
}