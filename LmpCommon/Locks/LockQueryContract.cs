using System;

namespace LmpCommon.Locks
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
            return LockExists(LockType.Contract, Guid.Empty, string.Empty);
        }

        /// <summary>
        /// Checks if the contract lock belongs to given player
        /// </summary>
        public bool ContractLockBelongsToPlayer(string playerName)
        {
            return LockBelongsToPlayer(LockType.Contract, Guid.Empty, string.Empty, playerName);
        }

        /// <summary>
        /// Gets the contract lock owner
        /// </summary>
        public string ContractLockOwner()
        {
            return GetLockOwner(LockType.Contract, Guid.Empty, string.Empty);
        }

        /// <summary>
        /// Gets the contract lock
        /// </summary>
        public LockDefinition ContractLock()
        {
            return GetLock(LockType.Contract, string.Empty, Guid.Empty, string.Empty);
        }
    }
}
