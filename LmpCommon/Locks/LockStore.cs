using System;
using System.Collections.Concurrent;

namespace LmpCommon.Locks
{
    /// <summary>
    /// This class serves to store all the defined locks
    /// </summary>
    public class LockStore
    {
        /// <summary>
        /// Provides a lock around modifications to the AsteroidCometLock object to ensure both atomic changes to the AsteroidCometLock
        /// and a memory barrier for changes to the AsteroidCometLock
        /// </summary>
        private readonly object _asteroidCometSyncLock = new object();

        /// <summary>
        /// Provides a lock around modifications to the ContractLock object to ensure both atomic changes to the ContractLock and a memory barrier for changes to the ContractLock
        /// </summary>
        private readonly object _contractSyncLock = new object();

        /// <summary>
        /// You can't have more than one user with the contract lock so it's a simple object
        /// </summary>
        internal LockDefinition ContractLock { get; set; }

        /// <summary>
        /// You can't have more than one user with the asteroid/comet lock so it's a simple object
        /// </summary>
        internal LockDefinition AsteroidCometLock { get; set; }

        /// <summary>
        /// Several users can have several update locks but a vessel can only have 1 update lock
        /// </summary>
        internal ConcurrentDictionary<Guid, LockDefinition> UpdateLocks { get; } = new ConcurrentDictionary<Guid, LockDefinition>();

        /// <summary>
        /// Several users can have several unloaded update locks but a vessel can only have 1 update lock
        /// </summary>
        internal ConcurrentDictionary<Guid, LockDefinition> UnloadedUpdateLocks { get; } = new ConcurrentDictionary<Guid, LockDefinition>();

        /// <summary>
        /// Several users can have several control locks but a vessel can only have 1 control lock
        /// </summary>
        internal ConcurrentDictionary<Guid, LockDefinition> ControlLocks { get; } = new ConcurrentDictionary<Guid, LockDefinition>();

        /// <summary>
        /// Several users can have several kerbal locks but a kerbal can only have 1 lock
        /// </summary>
        internal ConcurrentDictionary<string, LockDefinition> KerbalLocks { get; } = new ConcurrentDictionary<string, LockDefinition>();

        /// <summary>
        /// Several vessels can have several spectators locks but a user can only have 1 spectator lock
        /// </summary>
        internal ConcurrentDictionary<string, LockDefinition> SpectatorLocks { get; } = new ConcurrentDictionary<string, LockDefinition>();

        /// <summary>
        /// Adds or replace the given lock to the storage
        /// </summary>
        public void AddOrUpdateLock(LockDefinition lockDefinition)
        {
            //Be sure to clone the lock before storing to avoid any reference issue
            var safeLockDefinition = (LockDefinition)lockDefinition.Clone();

            switch (safeLockDefinition.Type)
            {
                case LockType.AsteroidComet:
                    lock (_asteroidCometSyncLock)
                    {
                        if (AsteroidCometLock == null)
                            AsteroidCometLock = new LockDefinition(LockType.AsteroidComet, safeLockDefinition.PlayerName);
                        else
                            AsteroidCometLock.PlayerName = safeLockDefinition.PlayerName;
                    }
                    break;
                case LockType.Kerbal:
                    KerbalLocks.AddOrUpdate(safeLockDefinition.KerbalName, safeLockDefinition, (key, existingVal) =>
                    {
                        existingVal.PlayerName = safeLockDefinition.PlayerName;
                        return existingVal;
                    });
                    break;
                case LockType.Update:
                    UpdateLocks.AddOrUpdate(safeLockDefinition.VesselId, safeLockDefinition, (key, existingVal) =>
                    {
                        existingVal.PlayerName = safeLockDefinition.PlayerName;
                        return existingVal;
                    });
                    break;
                case LockType.UnloadedUpdate:
                    UnloadedUpdateLocks.AddOrUpdate(safeLockDefinition.VesselId, safeLockDefinition, (key, existingVal) =>
                    {
                        existingVal.PlayerName = safeLockDefinition.PlayerName;
                        return existingVal;
                    });
                    break;
                case LockType.Control:
                    ControlLocks.AddOrUpdate(safeLockDefinition.VesselId, safeLockDefinition, (key, existingVal) =>
                    {
                        existingVal.PlayerName = safeLockDefinition.PlayerName;
                        return existingVal;
                    });
                    break;
                case LockType.Spectator:
                    SpectatorLocks.AddOrUpdate(safeLockDefinition.PlayerName, safeLockDefinition, (key, existingVal) => safeLockDefinition);
                    break;
                case LockType.Contract:
                    lock (_contractSyncLock)
                    {
                        if (ContractLock == null)
                            ContractLock = new LockDefinition(LockType.Contract, safeLockDefinition.PlayerName);
                        else
                            ContractLock.PlayerName = safeLockDefinition.PlayerName;
                    }
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
                case LockType.AsteroidComet:
                    lock (_asteroidCometSyncLock)
                    {
                        AsteroidCometLock = null;
                    }
                    break;
                case LockType.Kerbal:
                    KerbalLocks.TryRemove(lockDefinition.KerbalName, out _);
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
                case LockType.Contract:
                    lock (_contractSyncLock)
                    {
                        ContractLock = null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Removes given lock from storage
        /// </summary>
        public void RemoveLock(LockType lockType, string playerName, Guid vesselId, string kerbalName)
        {
            switch (lockType)
            {
                case LockType.AsteroidComet:
                    lock (_asteroidCometSyncLock)
                    {
                        AsteroidCometLock = null;
                    }
                    break;
                case LockType.Kerbal:
                    KerbalLocks.TryRemove(kerbalName, out _);
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
                case LockType.Contract:
                    lock (_contractSyncLock)
                    {
                        ContractLock = null;
                    }
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
            lock (_asteroidCometSyncLock)
            {
                AsteroidCometLock = null;
            }
            lock (_contractSyncLock)
            {
                ContractLock = null;
            }
            UpdateLocks.Clear();
            KerbalLocks.Clear();
            ControlLocks.Clear();
            SpectatorLocks.Clear();
            UnloadedUpdateLocks.Clear();
        }
    }
}
