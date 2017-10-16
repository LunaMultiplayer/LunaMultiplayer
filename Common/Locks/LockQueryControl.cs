using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for control locks
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if a control lock exists for given vessel
        /// </summary>
        public bool ControlLockExists(Guid vesselId)
        {
            return LockExists(LockType.Control, vesselId);
        }

        /// <summary>
        /// Checks if a control lock exists for given vessel and if so if it belongs to given player
        /// </summary>
        public bool ControlLockBelongsToPlayer(Guid vesselId, string playerName)
        {
            return ControlLockExists(vesselId) && LockBelongsToPlayer(LockType.Control, vesselId, playerName);
        }

        /// <summary>
        /// Get control lock owner for given vessel
        /// </summary>
        public string GetControlLockOwner(Guid vesselId)
        {
            return ControlLockExists(vesselId) ? GetLockOwner(LockType.Control, vesselId) : null;
        }

        /// <summary>
        /// Get all the control locks for given player
        /// </summary>
        public IEnumerable<LockDefinition> GetAllControlLocks(string playerName)
        {
            return LockStore.ControlLocks
                .Where(v => v.Value.PlayerName == playerName)
                .Select(v => v.Value);
        }

        /// <summary>
        /// Get all the control locks of all players
        /// </summary>
        public IEnumerable<LockDefinition> GetAllControlLocks()
        {
            return LockStore.ControlLocks.Select(v => v.Value);
        }
    }
}
