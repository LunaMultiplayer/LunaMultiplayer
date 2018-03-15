using System;
using System.Collections.Generic;

namespace LunaCommon.Locks
{
    /// <summary>
    /// Class that retrieve locks
    /// </summary>
    public partial class LockQuery
    {
        private LockStore LockStore { get; }

        public LockQuery(LockStore lockStore)
        {
            LockStore = lockStore;
        }

        /// <summary>
        /// Checks if the vessel based lock belongs to player
        /// </summary>
        public bool LockBelongsToPlayer(LockType type, Guid vesselId, string playerName)
        {
            switch (type)
            {
                case LockType.Spectator:
                    return SpectatorLockExists(playerName);
                case LockType.Asteroid:
                    return LockStore.AsteroidLock?.PlayerName == playerName;
                case LockType.Control:
                    if (LockStore.ControlLocks.TryGetValue(vesselId, out var controlLock))
                        return controlLock.Type == LockType.Control && controlLock.VesselId == vesselId && controlLock.PlayerName == playerName;
                    break;
                case LockType.Update:
                    if (LockStore.UpdateLocks.TryGetValue(vesselId, out var updateLock))
                        return updateLock.Type == LockType.Update && updateLock.VesselId == vesselId && updateLock.PlayerName == playerName;
                    break;
                case LockType.UnloadedUpdate:
                    if (LockStore.UnloadedUpdateLocks.TryGetValue(vesselId, out var unloadedUpdateLock))
                        return unloadedUpdateLock.Type == LockType.UnloadedUpdate && unloadedUpdateLock.VesselId == vesselId && unloadedUpdateLock.PlayerName == playerName;
                    break;
                case LockType.Contract:
                    return LockStore.ContractLock?.PlayerName == playerName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return false;
        }

        /// <summary>
        /// Checks if the vessel based lock exists
        /// </summary>
        public bool LockExists(LockType type, Guid vesselId)
        {
            switch (type)
            {
                case LockType.Asteroid:
                    return LockStore.AsteroidLock != null;
                case LockType.Control:
                    return LockStore.ControlLocks.ContainsKey(vesselId);
                case LockType.Update:
                    return LockStore.UpdateLocks.ContainsKey(vesselId);
                case LockType.UnloadedUpdate:
                    return LockStore.UnloadedUpdateLocks.ContainsKey(vesselId);
                case LockType.Contract:
                    return LockStore.ContractLock != null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Get the vessel based lock owner
        /// </summary>
        private string GetLockOwner(LockType type, Guid vesselId)
        {
            switch (type)
            {
                case LockType.Asteroid:
                    return LockStore.AsteroidLock?.PlayerName;
                case LockType.Control:
                    if (LockStore.ControlLocks.TryGetValue(vesselId, out var controlLock))
                        return controlLock.PlayerName;
                    break;
                case LockType.Update:
                    if (LockStore.UpdateLocks.TryGetValue(vesselId, out var updateLock))
                        return updateLock.PlayerName;
                    break;
                case LockType.UnloadedUpdate:
                    if (LockStore.UnloadedUpdateLocks.TryGetValue(vesselId, out var unloadedUpdateLock))
                        return unloadedUpdateLock.PlayerName;
                    break;
                case LockType.Contract:
                    return LockStore.ContractLock?.PlayerName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return null;
        }

        /// <summary>
        /// Get all the locks of a player
        /// </summary>
        public IEnumerable<LockDefinition> GetAllPlayerLocks(string playerName)
        {
            var locks = new List<LockDefinition>();
            locks.AddRange(GetAllControlLocks(playerName));
            locks.AddRange(GetAllUpdateLocks(playerName));
            locks.AddRange(GetAllUnloadedUpdateLocks(playerName));

            if (LockStore.SpectatorLocks.TryGetValue(playerName, out var spectatorLock))
                locks.Add(spectatorLock);

            if (AsteroidLockBelongsToPlayer(playerName))
                locks.Add(LockStore.AsteroidLock);

            return locks;
        }

        /// <summary>
        /// Get all the locks in the dictionaries
        /// </summary>
        public IEnumerable<LockDefinition> GetAllLocks()
        {
            var locks = new List<LockDefinition>();
            locks.AddRange(GetAllControlLocks());
            locks.AddRange(GetAllUpdateLocks());
            locks.AddRange(GetAllUnloadedUpdateLocks());
            locks.AddRange(LockStore.SpectatorLocks.Values);

            if (LockStore.AsteroidLock != null)
                locks.Add(LockStore.AsteroidLock);

            return locks;
        }

        /// <summary>
        /// Checks if the given lock exists
        /// </summary>
        public bool LockExists(LockDefinition lockDefinition)
        {
            switch (lockDefinition.Type)
            {
                case LockType.Asteroid:
                    return LockStore.AsteroidLock != null;
                case LockType.Update:
                    return LockStore.UpdateLocks.ContainsKey(lockDefinition.VesselId);
                case LockType.UnloadedUpdate:
                    return LockStore.UnloadedUpdateLocks.ContainsKey(lockDefinition.VesselId);
                case LockType.Control:
                    return LockStore.ControlLocks.ContainsKey(lockDefinition.VesselId);
                case LockType.Spectator:
                    return LockStore.SpectatorLocks.ContainsKey(lockDefinition.PlayerName);
                case LockType.Contract:
                    return LockStore.ContractLock != null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Retrieves a lock from the dictioanry based on the given lock
        /// </summary>
        public LockDefinition GetLock(LockType lockType, string playerName, Guid vesselId)
        {
            LockDefinition existingLock;
            switch (lockType)
            {
                case LockType.Asteroid:
                    return LockStore.AsteroidLock;
                case LockType.Update:
                    LockStore.UpdateLocks.TryGetValue(vesselId, out existingLock);
                    break;
                case LockType.UnloadedUpdate:
                    LockStore.UnloadedUpdateLocks.TryGetValue(vesselId, out existingLock);
                    break;
                case LockType.Control:
                    LockStore.ControlLocks.TryGetValue(vesselId, out existingLock);
                    break;
                case LockType.Spectator:
                    LockStore.SpectatorLocks.TryGetValue(playerName, out existingLock);
                    break;
                case LockType.Contract:
                    return LockStore.ContractLock;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return existingLock;
        }

        /// <summary>
        /// Check if player can remove the vessel from a terminated/recovered state
        /// </summary>
        public bool CanRecoverOrTerminateTheVessel(Guid vesselId, string playerName)
        {
            return !ControlLockExists(vesselId) || ControlLockBelongsToPlayer(vesselId, playerName);
        }
    }
}
