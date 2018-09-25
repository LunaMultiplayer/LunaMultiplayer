using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.Lock;
using LmpClient.Systems.Warp;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.Systems.Asteroid
{
    public class AsteroidSystem : System<AsteroidSystem>
    {
        #region Fields

        public AsteroidEvents AsteroidEvents { get; } = new AsteroidEvents();
        public bool AsteroidSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(AsteroidSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            
            TrackingEvent.onStartTrackingAsteroid.Add(AsteroidEvents.StartTrackingAsteroid);
            TrackingEvent.onStopTrackingAsteroid.Add(AsteroidEvents.StopTrackingAsteroid);
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, TryGetAsteroidLock));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            TrackingEvent.onStartTrackingAsteroid.Remove(AsteroidEvents.StartTrackingAsteroid);
            TrackingEvent.onStopTrackingAsteroid.Remove(AsteroidEvents.StopTrackingAsteroid);
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

        #endregion

        #region Public methods

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

            return protoVessel.protoPartSnapshots?.FirstOrDefault()?.partName == "PotatoRoid";
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
