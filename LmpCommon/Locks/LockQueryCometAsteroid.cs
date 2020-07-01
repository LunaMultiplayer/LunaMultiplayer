using System;

namespace LmpCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for asteroids
    /// </summary>
    public partial class LockQuery
    {
        /// <summary>
        /// Checks if the asteroid/comet lock is assigned
        /// </summary>
        public bool AsteroidCometLockExists()
        {
            return LockExists(LockType.AsteroidComet, Guid.Empty, string.Empty);
        }

        /// <summary>
        /// Checks if the asteroid/comet lock belongs to given player
        /// </summary>
        public bool AsteroidCometLockBelongsToPlayer(string playerName)
        {
            return LockBelongsToPlayer(LockType.AsteroidComet, Guid.Empty, string.Empty, playerName);
        }

        /// <summary>
        /// Gets the asteroid/comet lock owner
        /// </summary>
        public string AsteroidCometLockOwner()
        {
            return GetLockOwner(LockType.AsteroidComet, Guid.Empty, string.Empty);
        }

        /// <summary>
        /// Gets the asteroid/comet lock
        /// </summary>
        public LockDefinition AsteroidCometLock()
        {
            return GetLock(LockType.AsteroidComet, string.Empty, Guid.Empty, string.Empty);
        }
    }
}
