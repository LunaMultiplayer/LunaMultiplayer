using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LunaServer.Settings;

namespace LunaServer.System
{
    public class LockSystem
    {
        private static readonly ConcurrentDictionary<string, string> PlayerLocks =
            new ConcurrentDictionary<string, string>();

        //Lock types
        //control-vessel-(vesselid) - Replaces the old "inUse" messages, the active pilot will have the control-vessel lock.
        //update-vessel-(vesselid) - Replaces the "only the closest player can update a vessel" code, Now you acquire locks to update crafts around you.
        //asteroid - Held by the player that can spawn asteroids into the game.

        public static bool AcquireLock(string lockName, string playerName, bool force)
        {
            if (force || !PlayerLocks.ContainsKey(lockName))
            {
                PlayerLocks[lockName] = playerName;
                return true;
            }
            return false;
        }

        public static bool ReleaseLock(string lockName, string playerName)
        {
            if (PlayerLocks.ContainsKey(lockName) && (PlayerLocks[lockName] == playerName))
            {
                string value;
                PlayerLocks.TryRemove(lockName, out value);
                return true;
            }
            return false;
        }

        public static void ReleasePlayerLocks(string playerName)
        {
            //He is gone so he's not gonna spawn more asteroids...
            ReleaseLock("asteroid", playerName);

            var removeList = new List<string>();
            removeList.AddRange(PlayerLocks.Where(p => p.Value == playerName).Select(p => p.Key));

            foreach (var lockToRemove in removeList)
            {
                string value;
                if (lockToRemove.StartsWith("control-") && !GeneralSettings.SettingsStore.DropControlOnExit)
                    continue;
                PlayerLocks.TryRemove(lockToRemove, out value);
            }
        }

        public static KeyValuePair<string, string>[] GetLockList()
        {
            return PlayerLocks.ToArray();
        }

        public static void Reset()
        {
            PlayerLocks.Clear();
        }
    }
}