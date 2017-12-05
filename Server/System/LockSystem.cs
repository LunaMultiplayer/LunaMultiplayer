using LunaCommon.Locks;
using Server.Settings;

namespace Server.System
{
    public class LockSystem
    {
        private static readonly LockStore LockStore = new LockStore();
        public static readonly LockQuery LockQuery = new LockQuery(LockStore);
        
        public static bool AcquireLock(LockDefinition lockDef, bool force)
        {
            if (force || !LockQuery.LockExists(lockDef))
            {
                LockStore.AddOrUpdateLock(lockDef);
                return true;
            }
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

        public static void ReleasePlayerLocks(string playerName)
        {
            var removeList = LockQuery.GetAllPlayerLocks(playerName);

            foreach (var lockToRemove in removeList)
            {
                if (lockToRemove.Type == LockType.Control && !GeneralSettings.SettingsStore.DropControlOnExit)
                    continue;

                LockSystemSender.ReleaseAndSendLockReleaseMessage(lockToRemove);
            }
        }
    }
}