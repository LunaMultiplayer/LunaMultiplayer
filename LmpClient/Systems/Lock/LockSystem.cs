using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Locks;
using LmpCommon.Message.Data.Lock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LmpClient.Systems.Lock
{
    /// <summary>
    /// This system control the locks
    /// </summary>
    public class LockSystem : MessageSystem<LockSystem, LockMessageSender, LockMessageHandler>
    {
        public static LockStore LockStore { get; } = new LockStore();
        public static LockQuery LockQuery { get; } = new LockQuery(LockStore);

        #region Base overrides

        public override string SystemName { get; } = nameof(LockSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            LockStore.ClearAllLocks();
        }

        #endregion

        #region Public methods

        #region AcquireLocks

        /// <summary>
        /// Aquire the specified lock by sending a message to the server.
        /// </summary>
        /// <param name="lockDefinition">The definition of the lock to acquire</param>
        /// <param name="force">Force the aquire. Usually false unless in dockings.</param>
        private void AcquireLock(LockDefinition lockDefinition, bool force = false)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<LockAcquireMsgData>();
            msgData.Lock = lockDefinition;
            msgData.Force = force;

            MessageSender.SendMessage(msgData);
        }

        /// <summary>
        /// Aquire the control lock on the given vessel
        /// </summary>
        public void AcquireControlLock(Guid vesselId, bool force = false)
        {
            if (!LockQuery.ControlLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.Control, SettingsSystem.CurrentSettings.PlayerName, vesselId), force);
        }

        /// <summary>
        /// Aquire the kerbal lock on the given kerbal
        /// </summary>
        public void AcquireKerbalLock(string kerbalName, bool force = false)
        {
            if (!LockQuery.KerbalLockBelongsToPlayer(kerbalName, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.Kerbal, SettingsSystem.CurrentSettings.PlayerName, kerbalName), force);
        }

        /// <summary>
        /// Aquire the kerbal lock on the given vessel
        /// </summary>
        public void AcquireKerbalLock(Vessel vessel, bool force = false)
        {
            if (vessel == null) return;
            foreach (var kerbal in vessel.GetVesselCrew())
            {
                if (kerbal == null) continue;
                if (!LockQuery.KerbalLockBelongsToPlayer(kerbal.name, SettingsSystem.CurrentSettings.PlayerName))
                    AcquireLock(new LockDefinition(LockType.Kerbal, SettingsSystem.CurrentSettings.PlayerName, kerbal.name), force);
            }
        }

        /// <summary>
        /// Aquire the kerbal lock on the given vessel
        /// </summary>
        public void AcquireKerbalLock(Guid vesselId, bool force = false)
        {
            AcquireKerbalLock(FlightGlobals.fetch.LmpFindVessel(vesselId), force);
        }

        /// <summary>
        /// Aquire the update lock on the given vessel
        /// </summary>
        public void AcquireUpdateLock(Guid vesselId, bool force = false)
        {
            if (!LockQuery.UpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.Update, SettingsSystem.CurrentSettings.PlayerName, vesselId), force);
        }

        /// <summary>
        /// Aquire the unloaded update lock on the given vessel
        /// </summary>
        public void AcquireUnloadedUpdateLock(Guid vesselId, bool force = false)
        {
            if (!LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.UnloadedUpdate, SettingsSystem.CurrentSettings.PlayerName, vesselId), force);
        }

        /// <summary>
        /// Aquire the spectator lock on the given vessel
        /// </summary>
        public void AcquireSpectatorLock()
        {
            if (!LockQuery.SpectatorLockExists(SettingsSystem.CurrentSettings.PlayerName))
                AcquireLock(new LockDefinition(LockType.Spectator, SettingsSystem.CurrentSettings.PlayerName));
        }

        /// <summary>
        /// Aquire the asteroid lock for the current player
        /// </summary>
        public void AcquireAsteroidLock()
        {
            AcquireLock(new LockDefinition(LockType.Asteroid, SettingsSystem.CurrentSettings.PlayerName));
        }

        /// <summary>
        /// Acquire the contract lock for the current player.
        /// </summary>
        public void AcquireContractLock()
        {
            AcquireLock(new LockDefinition(LockType.Contract, SettingsSystem.CurrentSettings.PlayerName));
        }

        #endregion

        #region ReleaseLocks      

        /// <summary>
        /// Release the specified lock by sending a message to the server.
        /// </summary>
        private void ReleaseLock(LockDefinition lockDefinition)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<LockReleaseMsgData>();
            msgData.Lock = lockDefinition;

            LockStore.RemoveLock(lockDefinition);
            LockEvent.onLockRelease.Fire(lockDefinition);

            MessageSender.SendMessage(msgData);
        }

        /// <summary>
        /// Release the update lock on the given vessel
        /// </summary>
        public void ReleaseUpdateLock(Guid vesselId, uint vesselPersistentId)
        {
            ReleaseLock(new LockDefinition(LockType.Update, SettingsSystem.CurrentSettings.PlayerName, vesselId));
        }

        /// <summary>
        /// Release the given kerbal lock
        /// </summary>
        public void ReleaseKerbalLock(string kerbalName, int msDelay = 0)
        {
            TaskFactory.StartNew(() =>
            {
                if (msDelay > 0)
                    Thread.Sleep(msDelay);

                ReleaseLock(new LockDefinition(LockType.Kerbal, SettingsSystem.CurrentSettings.PlayerName, kerbalName));
            });
        }

        /// <summary>
        /// Release the spectator lock
        /// </summary>
        public void ReleaseSpectatorLock()
        {
            ReleaseLock(new LockDefinition(LockType.Spectator, SettingsSystem.CurrentSettings.PlayerName));
        }

        /// <summary>
        /// Release all the locks (unloaded update, update, control and kerbals) of a vessel
        /// </summary>
        public void ReleaseAllVesselLocks(IEnumerable<string> crewNames, Guid vesselId, int msDelay = 0)
        {
            TaskFactory.StartNew(() =>
            {
                if (msDelay > 0)
                    Thread.Sleep(msDelay);

                if (LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                    ReleaseLock(new LockDefinition(LockType.UnloadedUpdate, SettingsSystem.CurrentSettings.PlayerName, vesselId));
                if (LockQuery.UpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                    ReleaseLock(new LockDefinition(LockType.Update, SettingsSystem.CurrentSettings.PlayerName, vesselId));
                if (LockQuery.ControlLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                    ReleaseLock(new LockDefinition(LockType.Control, SettingsSystem.CurrentSettings.PlayerName, vesselId));

                if (crewNames != null)
                {
                    foreach (var kerbal in crewNames)
                    {
                        if (LockQuery.KerbalLockBelongsToPlayer(kerbal, SettingsSystem.CurrentSettings.PlayerName))
                            ReleaseLock(new LockDefinition(LockType.Kerbal, SettingsSystem.CurrentSettings.PlayerName, kerbal));
                    }
                }
            });
        }

        /// <summary>
        /// Release the specified control lock excepts the one for the vessel you give as parameter.
        /// </summary>
        public void ReleaseControlLocksExcept(Guid vesselId)
        {
            //Serialize to array as it's a concurrent collection
            var locksToRemove = LockQuery.GetAllControlLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(v => v.VesselId != vesselId).ToArray();

            foreach (var lockToRemove in locksToRemove)
            {
                ReleaseLock(lockToRemove);
            }
        }

        /// <summary>
        /// Releases all the player locks
        /// </summary>
        public void ReleaseAllPlayerSpecifiedLocks(params LockType[] lockTypes)
        {
            foreach (var lockToRelease in LockQuery.GetAllPlayerLocks(SettingsSystem.CurrentSettings.PlayerName))
            {
                if (lockTypes.Contains(lockToRelease.Type))
                    ReleaseLock(lockToRelease);
            }
        }

        /// <summary>
        /// Release all the locks you have based by type
        /// </summary>
        public void ReleasePlayerLocks(LockType type)
        {
            var playerName = SettingsSystem.CurrentSettings.PlayerName;
            IEnumerable<LockDefinition> locksToRelease;
            switch (type)
            {
                case LockType.Asteroid:
                    locksToRelease = LockQuery.AsteroidLockOwner() == playerName ?
                        new[] { LockQuery.AsteroidLock() } : new LockDefinition[0];
                    break;
                case LockType.Control:
                    locksToRelease = LockQuery.GetAllControlLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.Kerbal:
                    locksToRelease = LockQuery.GetAllKerbalLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.Update:
                    locksToRelease = LockQuery.GetAllUpdateLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.UnloadedUpdate:
                    locksToRelease = LockQuery.GetAllUnloadedUpdateLocks(SettingsSystem.CurrentSettings.PlayerName);
                    break;
                case LockType.Spectator:
                    locksToRelease = LockQuery.SpectatorLockExists(SettingsSystem.CurrentSettings.PlayerName) ?
                        new[] { LockQuery.GetSpectatorLock(SettingsSystem.CurrentSettings.PlayerName) } : new LockDefinition[0];
                    break;
                case LockType.Contract:
                    locksToRelease = LockQuery.ContractLockOwner() == playerName ?
                        new[] { LockQuery.ContractLock() } : new LockDefinition[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            //Serialize to array as it's a concurrent collection
            foreach (var lockToRelease in locksToRelease.ToArray())
            {
                ReleaseLock(lockToRelease);
            }
        }

        #endregion

        #endregion
    }
}
