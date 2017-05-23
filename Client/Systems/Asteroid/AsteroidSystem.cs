using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.Warp;
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
                {
                    foreach (var psm in HighLogic.CurrentGame.scenarios
                        .Where(psm => (psm?.moduleName == "ScenarioDiscoverableObjects") && (psm.moduleRef != null)))
                    {
                        _scenarioDiscoverableObjects = (ScenarioDiscoverableObjects)psm.moduleRef;
                        ScenarioController.spawnInterval = float.MaxValue;
                    }
                }
                return _scenarioDiscoverableObjects;
            }
            set { _scenarioDiscoverableObjects = value; }
        }

        public List<string> ServerAsteroids { get; } = new List<string>();
        public Dictionary<string, string> ServerAsteroidTrackStatus { get; } = new Dictionary<string, string>();
        private AsteroidEventHandler AsteroidEventHandler { get; } = new AsteroidEventHandler();
        
        #endregion
        
        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onAsteroidSpawned.Add(AsteroidEventHandler.OnAsteroidSpawned);
            GameEvents.onGameSceneLoadRequested.Add(AsteroidEventHandler.OnGameSceneLoadRequested);
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, CheckAsteroids));
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onAsteroidSpawned.Remove(AsteroidEventHandler.OnAsteroidSpawned);
            GameEvents.onGameSceneLoadRequested.Remove(AsteroidEventHandler.OnGameSceneLoadRequested);

            ServerAsteroids.Clear();
            ServerAsteroidTrackStatus.Clear();
        }

        #endregion

        #region Update methods

        /// <summary>
        /// This routine tries to ackquire the asteroid lock. If we have it spawn the needed asteroids.
        /// It also handles the asteroid track status between clients
        /// </summary>
        /// <returns></returns>
        private void CheckAsteroids()
        {
            if (!Enabled) return;

            //Try to acquire the asteroid-spawning lock if nobody else has it.
            if (!LockSystem.Singleton.LockExists("asteroid"))
                LockSystem.Singleton.AcquireLock("asteroid");

            //We have the spawn lock, lets do stuff.
            if (LockSystem.Singleton.LockIsOurs("asteroid") && WarpSystem.Singleton.CurrentSubspace == 0 &&
                Time.timeSinceLevelLoad > 1f && MainSystem.Singleton.GameRunning)
            {
                var beforeSpawn = GetAsteroidCount();
                var asteroidsToSpawn = SettingsSystem.ServerSettings.MaxNumberOfAsteroids - beforeSpawn;
                for (var asteroidsSpawned = 0; asteroidsSpawned < asteroidsToSpawn; asteroidsSpawned++)
                {
                    Debug.Log($"[LMP]: Spawning asteroid, have {beforeSpawn + asteroidsSpawned}, need {SettingsSystem.ServerSettings.MaxNumberOfAsteroids}");
                    ScenarioController.SpawnAsteroid();
                }
            }

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
                        Debug.Log($"[LMP]: Sending changed asteroid, new state: {asteroid.DiscoveryInfo.trackingStatus.Value}!");
                        ServerAsteroidTrackStatus[asteroid.id.ToString()] = asteroid.DiscoveryInfo.trackingStatus.Value;
                        VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid);
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
            if ((vessel != null) && !vessel.loaded)
                return ProtoVesselIsAsteroid(vessel.protoVessel);

            //Check the vessel has exactly one part.
            return (vessel?.parts != null) && (vessel.parts.Count == 1) && (vessel.parts[0].partName == "PotatoRoid");
        }

        public bool ProtoVesselIsAsteroid(ProtoVessel protoVessel)
        {
            return (protoVessel?.protoPartSnapshots != null) && (protoVessel.protoPartSnapshots.Count == 1) &&
                   (protoVessel.protoPartSnapshots[0].partName == "PotatoRoid");
        }

        public int GetAsteroidCount()
        {
            var seenAsteroids = new List<string>();

            seenAsteroids.AddRange(GetCurrentAsteroids().Select(a => a.id.ToString()));
            seenAsteroids.AddRange(HighLogic.CurrentGame.flightState.protoVessels
                .Where(a => ProtoVesselIsAsteroid(a) && !seenAsteroids.Contains(a.vesselID.ToString()))
                .Select(pv => pv.vesselID.ToString()));

            return seenAsteroids.Count;
        }

        public IEnumerable<Vessel> GetCurrentAsteroids()
        {
            return FlightGlobals.Vessels.Where(VesselIsAsteroid).ToList();
        }

        #endregion
    }
}