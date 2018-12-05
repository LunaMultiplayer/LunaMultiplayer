using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Systems.Lock;
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

            LockEvent.onLockRelease.Add(AsteroidEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Add(AsteroidEvents.LevelLoaded);
            TrackingEvent.onStartTrackingAsteroid.Add(AsteroidEvents.StartTrackingAsteroid);
            TrackingEvent.onStopTrackingAsteroid.Add(AsteroidEvents.StopTrackingAsteroid);
            GameEvents.onNewVesselCreated.Add(AsteroidEvents.NewVesselCreated);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            LockEvent.onLockRelease.Remove(AsteroidEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Remove(AsteroidEvents.LevelLoaded);
            TrackingEvent.onStartTrackingAsteroid.Remove(AsteroidEvents.StartTrackingAsteroid);
            TrackingEvent.onStopTrackingAsteroid.Remove(AsteroidEvents.StopTrackingAsteroid);
            GameEvents.onNewVesselCreated.Remove(AsteroidEvents.NewVesselCreated);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Try to acquire the asteroid-spawning lock if nobody else has it.
        /// </summary>
        public void TryGetAsteroidLock()
        {
            if (!LockSystem.LockQuery.AsteroidLockExists())
                LockSystem.Singleton.AcquireAsteroidLock();
        }

        public int GetAsteroidCount()
        {
            var seenAsteroids = GetCurrentAsteroids().Count();
            return seenAsteroids;
        }

        public IEnumerable<Vessel> GetCurrentAsteroids()
        {
            return FlightGlobals.Vessels.Where(v=> v.IsAsteroid());
        }

        #endregion
    }
}
