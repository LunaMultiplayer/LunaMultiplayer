using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.Warp;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.Asteroid
{
    public class AsteroidSystem : System<AsteroidSystem>
    {
        #region Fields

        public List<Guid> ServerAsteroids { get; } = new List<Guid>();
        public Dictionary<Guid, string> ServerAsteroidTrackStatus { get; } = new Dictionary<Guid, string>();

        public bool AsteroidSystemReady => Enabled && Time.timeSinceLevelLoad > 5f && HighLogic.LoadedScene >= GameScenes.FLIGHT;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(AsteroidSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, TryGetAsteroidLock));
            SetupRoutine(new RoutineDefinition(15000, RoutineExecution.Update, CheckAsteroidsStatus));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ServerAsteroids.Clear();
            ServerAsteroidTrackStatus.Clear();
        }

        #endregion

        #region Update methods
        
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
        /// This routine handles the asteroid track status between clients
        /// </summary>
        private void CheckAsteroidsStatus()
        {
            if (!Enabled || !AsteroidSystemReady) return;

            //Check for changes to tracking
            foreach (var asteroid in GetCurrentAsteroids().Where(asteroid => asteroid.state != Vessel.State.DEAD))
            {
                if (!ServerAsteroidTrackStatus.ContainsKey(asteroid.id))
                {
                    ServerAsteroidTrackStatus.Add(asteroid.id, asteroid.DiscoveryInfo.trackingStatus.Value);
                }
                else
                {
                    if (asteroid.DiscoveryInfo.trackingStatus.Value != ServerAsteroidTrackStatus[asteroid.id])
                    {
                        LunaLog.Log($"[LMP]: Sending changed asteroid, new state: {asteroid.DiscoveryInfo.trackingStatus.Value}!");
                        ServerAsteroidTrackStatus[asteroid.id] = asteroid.DiscoveryInfo.trackingStatus.Value;
                        VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid, false);
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
        public void RegisterServerAsteroid(Guid asteroidId)
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

        private static bool ProtoVesselIsAsteroid(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return false;

            if ((protoVessel.protoPartSnapshots == null || protoVessel.protoPartSnapshots.Count == 0) && protoVessel.vesselName.StartsWith("Ast."))
                return true;

            return protoVessel.protoPartSnapshots?[0].partName == "PotatoRoid";
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
