using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Message.Data.Lock;
using UnityEngine;

namespace LunaClient.Systems.Lock
{
    /// <summary>
    /// This system control the locks.
    /// Locks are "control", "update", "spectator-playername" and "asteroid"
    /// If you own the control lock then you can move that vessel.
    /// If you own the update lock you are the one who sends vessel updates (position, speed, etc) to the server
    /// If you own the asteroid lock then you spawn asteroids
    /// If you own the spectator lock then you are spectating
    /// The dictionary is defined as "LockName:PlayerName"
    /// </summary>
    public class LockSystem : MessageSystem<LockSystem, LockMessageSender, LockMessageHandler>
    {
        public Dictionary<string, string> ServerLocks { get; } = new Dictionary<string, string>();
        public List<AcquireEvent> LockAcquireEvents { get; } = new List<AcquireEvent>();
        public List<ReleaseEvent> LockReleaseEvents { get; } = new List<ReleaseEvent>();

        #region Base overrides

        public override void OnDisabled()
        {
            base.OnDisabled();
            ServerLocks.Clear();
            LockAcquireEvents.Clear();
            LockReleaseEvents.Clear();
        }

        #endregion

        #region Public methods

        #region Hooks

        #region RegisterHook

        public void RegisterAcquireHook(AcquireEvent methodObject)
        {
            LockAcquireEvents.Add(methodObject);
        }

        public void RegisterReleaseHook(ReleaseEvent methodObject)
        {
            LockReleaseEvents.Add(methodObject);
        }

        #endregion

        #region UnregisterHook

        public void UnregisterAcquireHook(AcquireEvent methodObject)
        {
            if (LockAcquireEvents.Contains(methodObject))
                LockAcquireEvents.Remove(methodObject);
        }

        public void UnregisterReleaseHook(ReleaseEvent methodObject)
        {
            if (LockReleaseEvents.Contains(methodObject))
                LockReleaseEvents.Remove(methodObject);
        }

        #endregion

        #endregion

        #region Locks

        #region Events

        /// <summary>
        /// This event is triggered when some player acquired a lock
        /// It then calls all the methods specified in the delegate
        /// </summary>
        public void FireAcquireEvent(string playerName, string lockName, bool lockResult)
        {
            foreach (var methodObject in LockAcquireEvents)
            {
                try
                {
                    methodObject(playerName, lockName, lockResult);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error thrown in acquire lock event, exception {e}");
                }
            }
        }

        /// <summary>
        /// This event is triggered when some player released a lock
        /// It then calls all the methods specified in the delegate
        /// </summary>
        public void FireReleaseEvent(string playerName, string lockName)
        {
            foreach (var methodObject in LockReleaseEvents)
            {
                try
                {
                    methodObject(playerName, lockName);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error thrown in release lock event, exception {e}");
                }
            }
        }

        #endregion

        #region AcquireLocks

        /// <summary>
        /// Aquire the specified lock by sending a message to the server.
        /// </summary>
        /// <param name="lockName">Name of the lock to aquire, normally control or update</param>
        /// <param name="force">Force the aquire. Usually false unless in dockings.</param>
        public void AcquireLock(string lockName, bool force = false)
        {
            MessageSender.SendMessage(new LockAcquireMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                LockName = lockName,
                Force = force
            });
        }

        /// <summary>
        /// Aquire the spectator lock on the given vessel
        /// </summary>
        public void AcquireSpectatorLock(Guid vesselId)
        {
            MessageSender.SendMessage(new LockAcquireMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                LockName = $"spectator-{SettingsSystem.CurrentSettings.PlayerName}-{vesselId}",
                Force = false
            });
        }

        /// <summary>
        /// Release the spectator locks
        /// </summary>
        public void ReleaseSpectatorLock()
        {
            var spectatorLocks = GetLocksWithPrefix($"spectator-{SettingsSystem.CurrentSettings.PlayerName}");

            foreach (var spectatorLock in spectatorLocks)
            {
                MessageSender.SendMessage(new LockReleaseMsgData
                {
                    PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                    LockName = spectatorLock,
                });
            }
        }

        #endregion

        #region ReleaseLocks      

        /// <summary>
        /// Release the specified control lock excepts the ones you pass. 
        /// You must send the full control lock name, not the prefix
        /// </summary>
        public void ReleaseControlLocksExcept(params string[] lockNames)
        {
            var removeList = ServerLocks
                .Where(l => !lockNames.Contains(l.Key) && l.Key.StartsWith("control-") && (l.Value == SettingsSystem.CurrentSettings.PlayerName))
                .Select(l => l.Key)
                .ToArray();

            foreach (var removeValue in removeList)
            {
                ReleaseLock(removeValue);
                ServerLocks.Remove(removeValue);
                FireReleaseEvent(SettingsSystem.CurrentSettings.PlayerName, removeValue);
            }
        }

        /// <summary>
        /// Release the specified lock
        /// </summary>
        public void ReleaseLock(string lockName)
        {
            if (LockIsOurs(lockName))
            {
                ServerLocks.Remove(lockName);
                MessageSender.SendMessage(new LockReleaseMsgData
                {
                    PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                    LockName = lockName
                });
            }
        }

        public void ReleaseLocksWithPrefix(string prefix)
        {
            var removeList = ServerLocks
                .Where(l => l.Key.StartsWith(prefix) && (l.Value == SettingsSystem.CurrentSettings.PlayerName))
                .Select(l => l.Key)
                .ToArray();

            foreach (var removeValue in removeList)
            {
                ReleaseLock(removeValue);
                ServerLocks.Remove(removeValue);
                FireReleaseEvent(SettingsSystem.CurrentSettings.PlayerName, removeValue);
            }
        }

        #endregion

        #region Query
        
        public bool LockIsOurs(string lockName)
        {
            return ServerLocks.ContainsKey(lockName) && (ServerLocks[lockName] == SettingsSystem.CurrentSettings.PlayerName);
        }

        public bool LockWithPrefixExists(string lockPrefix)
        {
            return ServerLocks.Any(l=> l.Key.StartsWith(lockPrefix));
        }

        public bool SpectatorLockExists(Guid vesselId)
        {
            return ServerLocks.Any(l => l.Key.EndsWith(vesselId.ToString()) && l.Key.StartsWith("spectator-"));
        }

        public bool LockExists(string lockName)
        {
            return ServerLocks.ContainsKey(lockName);
        }

        public string LockOwner(string lockName)
        {
            return ServerLocks.ContainsKey(lockName) ? ServerLocks[lockName] : "unknown player";
        }

        public string[] GetPlayerLocks(string playerName)
        {
            return ServerLocks.Where(l => l.Value == playerName).Select(l => l.Key).ToArray();
        }

        public string[] GetPlayerLocksPrefix(string playerName, string lockPrefix)
        {
            return ServerLocks.Where(l => l.Value == playerName && l.Key.StartsWith(lockPrefix)).Select(l => l.Key).ToArray();
        }

        public string[] GetLocksWithPrefix(string lockPrefix)
        {
            return ServerLocks.Where(l => l.Key.StartsWith(lockPrefix)).Select(l => l.Key).ToArray();
        }

        #endregion

        #endregion

        #endregion
    }
}