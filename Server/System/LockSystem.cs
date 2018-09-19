using LmpCommon.Locks;
using Server.Client;
using System.Linq;

namespace Server.System
{
    public class LockSystem
    {
        private static readonly LockStore LockStore = new LockStore();
        public static readonly LockQuery LockQuery = new LockQuery(LockStore);
        
        public static bool AcquireLock(LockDefinition lockDef, bool force, out bool repeatedAcquire)
        {
            repeatedAcquire = false;

            //Player tried to acquire a lock that he already owns
            if (LockQuery.LockBelongsToPlayer(lockDef.Type, lockDef.VesselId, lockDef.KerbalName, lockDef.PlayerName))
            {
                repeatedAcquire = true;
                return true;
            }

            if (force || !LockQuery.LockExists(lockDef))
            {
                if (lockDef.Type == LockType.Control)
                {
                    //If he acquired a control lock he probably switched vessels or smth like that and he can only have 1 control lock.
                    //So remove the other control locks just for safety...
                    var controlLocks = LockQuery.GetAllPlayerLocks(lockDef.PlayerName).Where(l => l.Type == LockType.Control);
                    foreach (var control in controlLocks)
                        ReleaseLock(control);
                }

                LockStore.AddOrUpdateLock(lockDef);
                return true;
            }
            return false;
        }

        public static bool ReleaseLock(LockDefinition lockDef)
        {
            if (LockQuery.LockBelongsToPlayer(lockDef.Type, lockDef.VesselId, lockDef.KerbalName, lockDef.PlayerName))
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
                LockSystemSender.ReleaseAndSendLockReleaseMessage(client, lockToRemove);
            }
        }
    }
}
