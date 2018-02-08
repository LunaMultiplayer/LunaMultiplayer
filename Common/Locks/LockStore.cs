using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LunaCommon.Locks
{
    /// <summary>
    /// This class serves to store all the defined locks
    /// </summary>
    public class LockStore
    {
        /// <summary>
        /// Provides a lock around modifications to the AsteroidLock object to ensure both atomic changes to the AsteroidLock and a memory barrier for changes to the AsteroidLock
        /// </summary>
        private readonly object _asteroidSyncLock = new object();

        /// <summary>
        /// You can't have more than one user with the asteroid lock so it's a simple object
        /// </summary>
        internal LockDefinition AsteroidLock { get; set; }

        /// <summary>
        /// Several users can have several update locks but a vessel can only have 1 update lock
        /// </summary>
        internal ConcurrentDictionary<Guid, LockDefinition> UpdateLocks { get; set; } = new ConcurrentDictionary<Guid, LockDefinition>();

        /// <summary>
        /// Several users can have several unloaded update locks but a vessel can only have 1 update lock
        /// </summary>
        internal ConcurrentDictionary<Guid, LockDefinition> UnloadedUpdateLocks { get; set; } = new ConcurrentDictionary<Guid, LockDefinition>();

        /// <summary>
        /// Several users can have several control locks but a vessel can only have 1 control lock
        /// </summary>
        internal ConcurrentDictionary<Guid, LockDefinition> ControlLocks { get; set; } = new ConcurrentDictionary<Guid, LockDefinition>();

        /// <summary>
        /// Several vessels can have several spectators locks but a user can only have 1 spectator lock
        /// </summary>
        internal ConcurrentDictionary<string, LockDefinition> SpectatorLocks { get; set; } = new ConcurrentDictionary<string, LockDefinition>();

        /// <summary>
        /// Adds or replace the given lock to the storage
        /// </summary>
        public void AddOrUpdateLock(LockDefinition lockDefinition)
        {
            switch (lockDefinition.Type)
            {
                case LockType.Asteroid:
                    lock (_asteroidSyncLock)
                    {
                        if (AsteroidLock == null)
                            AsteroidLock = new LockDefinition(LockType.Asteroid, lockDefinition.PlayerName);
                        else
                            AsteroidLock.PlayerName = lockDefinition.PlayerName;
                    }
                       break;
                case LockType.Update:
                    UpdateLocks.AddOrUpdate(lockDefinition.VesselId, lockDefinition, (key, existingVal) => lockDefinition);
                    break;
                case LockType.UnloadedUpdate:
                    UnloadedUpdateLocks.AddOrUpdate(lockDefinition.VesselId, lockDefinition, (key, existingVal) => lockDefinition);
                    break;
                case LockType.Control:
                    ControlLocks.AddOrUpdate(lockDefinition.VesselId, lockDefinition, (key, existingVal) => lockDefinition);
                    break;
                case LockType.Spectator:
                    SpectatorLocks.AddOrUpdate(lockDefinition.PlayerName, lockDefinition, (key, existingVal) => lockDefinition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Removes given lock from storage
        /// </summary>
        public void RemoveLock(LockDefinition lockDefinition)
        {
            switch (lockDefinition.Type)
            {
                case LockType.Asteroid:
                    lock (_asteroidSyncLock)
                    {
                        AsteroidLock = null;
                    }
                    break;
                case LockType.UnloadedUpdate:
                    UnloadedUpdateLocks.TryRemove(lockDefinition.VesselId, out _);
                    break;
                case LockType.Update:
                    UpdateLocks.TryRemove(lockDefinition.VesselId, out _);
                    break;
                case LockType.Control:
                    ControlLocks.TryRemove(lockDefinition.VesselId, out _);
                    break;
                case LockType.Spectator:
                    SpectatorLocks.TryRemove(lockDefinition.PlayerName, out _);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Removes given lock from storage
        /// </summary>
        public void RemoveLock(LockType lockType, string playerName, Guid vesselId)
        {
            switch (lockType)
            {
                case LockType.Asteroid:
                    lock (_asteroidSyncLock)
                    {
                        AsteroidLock = null;
                    }
                    break;
                case LockType.UnloadedUpdate:
                    UnloadedUpdateLocks.TryRemove(vesselId, out _);
                    break;
                case LockType.Update:
                    UpdateLocks.TryRemove(vesselId, out _);
                    break;
                case LockType.Control:
                    ControlLocks.TryRemove(vesselId, out _);
                    break;
                case LockType.Spectator:
                    SpectatorLocks.TryRemove(playerName, out _);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Clear all locks
        /// </summary>
        public void ClearAllLocks()
        {
            new Task(() =>
            {
                lock (_asteroidSyncLock)
                {
                    AsteroidLock = null;
                }
                UpdateLocks.Clear();
                ControlLocks.Clear();
                SpectatorLocks.Clear();
                UnloadedUpdateLocks.Clear();
            }).Start(TaskScheduler.Current);
        }


    }
}