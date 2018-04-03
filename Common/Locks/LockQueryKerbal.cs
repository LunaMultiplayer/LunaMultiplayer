using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for kerbal locks
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if a kerbal lock exists for given kerbal name
        /// </summary>
        public bool KerbalLockExists(string kerbalName)
        {
            return LockExists(LockType.Kerbal, Guid.Empty, kerbalName);
        }

        /// <summary>
        /// Checks if a kerbal lock exists for given kerbal name and if so if it belongs to given player
        /// </summary>
        public bool KerbalLockBelongsToPlayer(string kerbalName, string playerName)
        {
            return LockBelongsToPlayer(LockType.Kerbal, Guid.Empty, kerbalName, playerName);
        }

        /// <summary>
        /// Get kerbal lock owner for given kerbal name
        /// </summary>
        public string GetKerbalLockOwner(string kerbalName)
        {
            return GetLockOwner(LockType.Kerbal, Guid.Empty, kerbalName);
        }

        /// <summary>
        /// Get all the kerbal locks for given player
        /// </summary>
        public IEnumerable<LockDefinition> GetAllKerbalLocks(string playerName)
        {
            return LockStore.KerbalLocks.Select(v => v.Value)
                .Where(v => v.PlayerName == playerName);
        }

        /// <summary>
        /// Get all the kerbal locks of all players
        /// </summary>
        public IEnumerable<LockDefinition> GetAllKerbalLocks()
        {
            return LockStore.KerbalLocks.Select(v => v.Value);
        }
    }
}
