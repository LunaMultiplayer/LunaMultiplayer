using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.Warp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.Asteroid
{
    public class AsteroidSystem : System<AsteroidSystem>
    {
        #region Fields

        private ScenarioDiscoverableObjects _scenarioDiscoverableObjects;

        public ScenarioDiscoverableObjects ScenarioController
        {
            get
            {
                if (_scenarioDiscoverableObjects == null)
                    _scenarioDiscoverableObjects = Object.FindObjectsOfType<ScenarioDiscoverableObjects>().FirstOrDefault();

                return _scenarioDiscoverableObjects;
            }
        }


        public List<string> ServerAsteroids { get; } = new List<string>();
        public Dictionary<string, string> ServerAsteroidTrackStatus { get; } = new Dictionary<string, string>();
        private AsteroidEventHandler AsteroidEventHandler { get; } = new AsteroidEventHandler();

        public bool AsteroidSystemReady => Enabled && Time.timeSinceLevelLoad > 5f && HighLogic.LoadedScene >= GameScenes.FLIGHT;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(AsteroidSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onAsteroidSpawned.Add(AsteroidEventHandler.OnAsteroidSpawned);

            SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, FixAsteroidIntervalAndSeed));
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, TryGetAsteroidLock));
            SetupRoutine(new RoutineDefinition(15000, RoutineExecution.Update, CheckAsteroidsToSpawn));
            SetupRoutine(new RoutineDefinition(15000, RoutineExecution.Update, CheckAsteroidsStatus));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onAsteroidSpawned.Remove(AsteroidEventHandler.OnAsteroidSpawned);

            ServerAsteroids.Clear();
            ServerAsteroidTrackStatus.Clear();
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Here we try to fix the SQUAD error with the seed value and also we stop the automatic asteroid spawner
        /// </summary>
        private void FixAsteroidIntervalAndSeed()
        {
            if (!Enabled) return;

            if (ScenarioController != null)
            {
                ScenarioController.spawnOddsAgainst = int.MaxValue;
                ScenarioController.spawnInterval = float.MaxValue;
            }
        }

        /// <summary>
        /// Try to acquire the asteroid-spawning lock if nobody else has it.
        /// </summary>
        private void TryGetAsteroidLock()
        {
            if (!Enabled || !AsteroidSystemReady) return;

            if (!LockSystem.LockQuery.AsteroidLockExists() && WarpSystem.Singleton.CurrentSubspace == 0)
                LockSystem.Singleton.AcquireAsteroidLock();
        }

        /// <summary>
        /// This spawn the needed asteroids if we have the asteroid lock
        /// </summary>
        private void CheckAsteroidsToSpawn()
        {
            if (!Enabled || !AsteroidSystemReady) return;

            if (LockSystem.LockQuery.AsteroidLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
            {
                //We have the spawn lock so spawn some asteroids if there are less than expected
                var currentAsteroidCount = GetAsteroidCount();
                var asteroidsToSpawn = SettingsSystem.ServerSettings.MaxNumberOfAsteroids - currentAsteroidCount;

                if (asteroidsToSpawn > 0)
                {
                    LunaLog.Log($"[LMP]: Spawning {asteroidsToSpawn} asteroids");
                    for (var i = 0; i < asteroidsToSpawn; i++)
                    {
                        ScenarioController?.SpawnAsteroid();
                    }
                }
            }
        }

        /// <summary>
        /// This routine handles the asteroid track status between clients
        /// </summary>
        private void CheckAsteroidsStatus()
        {
            if (!Enabled || !AsteroidSystemReady) return;

            //Check for changes to tracking
            foreach (var asteroid in GetCurrentAsteroids().Where(asteroid => asteroid.state != Vessel.State.DEAD))
            {
                if (!ServerAsteroidTrackStatus.ContainsKey(asteroid.id.ToString()))
                {
                    ServerAsteroidTrackStatus.Add(asteroid.id.ToString(), asteroid.DiscoveryInfo.trackingStatus.Value);
                }
                else
                {
                    if (asteroid.DiscoveryInfo.trackingStatus.Value != ServerAsteroidTrackStatus[asteroid.id.ToString()])
                    {
                        LunaLog.Log($"[LMP]: Sending changed asteroid, new state: {asteroid.DiscoveryInfo.trackingStatus.Value}!");
                        ServerAsteroidTrackStatus[asteroid.id.ToString()] = asteroid.DiscoveryInfo.trackingStatus.Value;
                        VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid, true);
                    }
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Registers the server asteroid - Prevents LMP from deleting it.
        /// </summary>
        /// <param name="asteroidId">Asteroid to register</param>
        public void RegisterServerAsteroid(string asteroidId)
        {
            if (!ServerAsteroids.Contains(asteroidId))
                ServerAsteroids.Add(asteroidId);
            //This will ignore Status changes so we don't resend the asteroid.
            if (ServerAsteroidTrackStatus.ContainsKey(asteroidId))
                ServerAsteroidTrackStatus.Remove(asteroidId);
        }

        public bool VesselIsAsteroid(Vessel vessel)
        {
            if (vessel != null && !vessel.loaded)
                return ProtoVesselIsAsteroid(vessel.protoVessel);

            //Check the vessel has exactly one part.
            return vessel?.parts != null && vessel.parts.Count == 1 && vessel.parts[0].partName == "PotatoRoid";
        }

        public bool ProtoVesselIsAsteroid(ProtoVessel protoVessel)
        {
            return (protoVessel.protoPartSnapshots == null || protoVessel.protoPartSnapshots.Count == 0) && protoVessel.vesselName.StartsWith("Ast.")
                || protoVessel.protoPartSnapshots != null && protoVessel.protoPartSnapshots.Count == 1 &&
                   protoVessel.protoPartSnapshots[0].partName == "PotatoRoid";
        }

        public int GetAsteroidCount()
        {
            var seenAsteroids = GetCurrentAsteroids().Count();
            return seenAsteroids;
        }

        public IEnumerable<Vessel> GetCurrentAsteroids()
        {
            return FlightGlobals.Vessels.Where(VesselIsAsteroid);
        }

        #endregion
    }
}