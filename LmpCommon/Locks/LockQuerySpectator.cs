namespace LmpCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks specific for spectators
    /// </summary>
    public partial class LockQuery
    {
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
            return LockStore.SpectatorLocks.TryGetValue(playerName, out var spectatorLock) ? spectatorLock : null;
        }
    }
}
