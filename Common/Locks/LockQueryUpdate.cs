using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for update locks
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if a control lock exists for given vessel
        /// </summary>
        public bool UpdateLockExists(Guid vesselId)
        {
            return LockExists(LockType.Update, vesselId);
        }

        /// <summary>
        /// Checks if a control lock exists for given vessel and if so if it belongs to given player
        /// </summary>
        public bool UpdateLockBelongsToPlayer(Guid vesselId, string playerName)
        {
            return LockBelongsToPlayer(LockType.Update, vesselId, playerName);
        }

        /// <summary>
        /// Get update lock owner for given vessel
        /// </summary>
        public string GetUpdateLockOwner(Guid vesselId)
        {
            return GetLockOwner(LockType.Update, vesselId);
        }

        /// <summary>
        /// Get all the update locks for given player
        /// </summary>
        public IEnumerable<LockDefinition> GetAllUpdateLocks(string playerName)
        {
            return LockStore.UpdateLocks
                .Where(v => v.Value.PlayerName == playerName)
                .Select(v => v.Value);
        }

        /// <summary>
        /// Get all the update locks of all players
        /// </summary>
        public IEnumerable<LockDefinition> GetAllUpdateLocks()
        {
            return LockStore.UpdateLocks.Select(v => v.Value);
        }
    }
}
