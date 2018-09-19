using System;

namespace LmpCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for asteroids
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if the asteroid lock is assigned
        /// </summary>
        public bool AsteroidLockExists()
        {
            return LockExists(LockType.Asteroid, Guid.Empty, string.Empty);
        }

        /// <summary>
        /// Checks if the asteroid lock belongs to given player
        /// </summary>
        public bool AsteroidLockBelongsToPlayer(string playerName)
        {
            return LockBelongsToPlayer(LockType.Asteroid, Guid.Empty, string.Empty, playerName);
        }

        /// <summary>
        /// Gets the asteroid lock owner
        /// </summary>
        public string AsteroidLockOwner()
        {
            return GetLockOwner(LockType.Asteroid, Guid.Empty, string.Empty);
        }

        /// <summary>
        /// Gets the asteroid lock
        /// </summary>
        public LockDefinition AsteroidLock()
        {
            return GetLock(LockType.Asteroid, string.Empty, Guid.Empty, string.Empty);
        }
    }
}
