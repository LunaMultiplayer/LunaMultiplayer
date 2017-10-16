namespace LunaCommon.Locks
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
            return LockStore.AsteroidLock != null;
        }

        /// <summary>
        /// Checks if the asteroid lock belongs to given player
        /// </summary>
        public bool AsteroidLockBelongsToPlayer(string playerName)
        {
            return LockStore.AsteroidLock?.PlayerName == playerName;
        }

        /// <summary>
        /// Gets the asteroid lock owner
        /// </summary>
        public string AsteroidLockOwner()
        {
            return LockStore.AsteroidLock?.PlayerName;
        }

        /// <summary>
        /// Gets the asteroid lock
        /// </summary>
        public LockDefinition AsteroidLock()
        {
            return LockStore.AsteroidLock;
        }
    }
}
