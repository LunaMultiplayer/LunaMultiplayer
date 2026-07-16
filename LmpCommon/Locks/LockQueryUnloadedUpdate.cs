using System;
using System.Collections.Generic;
using System.Linq;

namespace LmpCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for update locks
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if a unloaded update lock exists for given vessel
        /// </summary>
        public bool UnloadedUpdateLockExists(Guid vesselId)
        {
            return LockExists(LockType.UnloadedUpdate, vesselId, string.Empty);
        }

        /// <summary>
        /// Checks if a unloaded update lock exists for given vessel and if so if it belongs to given player
        /// </summary>
        public bool UnloadedUpdateLockBelongsToPlayer(Guid vesselId, string playerName)
        {
            return LockBelongsToPlayer(LockType.UnloadedUpdate, vesselId, string.Empty, playerName);
        }

        /// <summary>
        /// Get unloaded update lock for given vessel
        /// </summary>
        public LockDefinition GetUnloadedUpdateLock(Guid vesselId)
        {
            return LockStore.UnloadedUpdateLocks.TryGetValue(vesselId, out var lockDef) ? lockDef : null;
        }

        /// <summary>
        /// Get unloaded update lock owner for given vessel
        /// </summary>
        public string GetUnloadedUpdateLockOwner(Guid vesselId)
        {
            return GetLockOwner(LockType.UnloadedUpdate, vesselId, string.Empty);
        }

        /// <summary>
        /// Get all the unloaded update locks for given player
        /// </summary>
        public IEnumerable<LockDefinition> GetAllUnloadedUpdateLocks(string playerName)
        {
            return LockStore.UnloadedUpdateLocks
                .Where(v => v.Value.PlayerName == playerName)
                .Select(v => v.Value);
        }

        /// <summary>
        /// Get all the unloaded update locks of all players
        /// </summary>
        public IEnumerable<LockDefinition> GetAllUnloadedUpdateLocks()
        {
            return LockStore.UnloadedUpdateLocks.Select(v => v.Value);
        }
    }
}
