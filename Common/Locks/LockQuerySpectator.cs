using System;
using System.Linq;

namespace LunaCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for spectators
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if there's an spectator for given vessel
        /// </summary>
        public bool SpectatorLockExists(Guid vesselId)
        {
            return LockStore.SpectatorLocks.Any(v => v.Value.VesselId == vesselId);
        }

        /// <summary>
        /// Checks if given user is spectating
        /// </summary>
        public bool SpectatorLockExists(string playerName)
        {
            return LockStore.SpectatorLocks.ContainsKey(playerName);
        }

        /// <summary>
        /// Gets the spectator lock for given player if it exists, otherwise null
        /// </summary>
        public LockDefinition GetSpectatorLock(string playerName)
        {
            return SpectatorLockExists(playerName) ? LockStore.SpectatorLocks[playerName] : null;
        }
    }
}
