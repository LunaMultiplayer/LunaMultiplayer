using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for contracts
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if the contract lock is assigned
        /// </summary>
        public bool ContractLockExists()
        {
            return LockExists(LockType.Contract, Guid.Empty);
        }

        /// <summary>
        /// Checks if the contract lock belongs to given player
        /// </summary>
        public bool ContractLockBelongsToPlayer(string playerName)
        {
            return LockBelongsToPlayer(LockType.Contract, Guid.Empty, playerName);
        }

        /// <summary>
        /// Gets the contract lock owner
        /// </summary>
        public string ContractLockOwner()
        {
            return GetLockOwner(LockType.Contract, Guid.Empty);
        }

        /// <summary>
        /// Gets the contract lock
        /// </summary>
        public LockDefinition ContractLock()
        {
            return GetLock(LockType.Contract, string.Empty, Guid.Empty);
        }
    }
}
