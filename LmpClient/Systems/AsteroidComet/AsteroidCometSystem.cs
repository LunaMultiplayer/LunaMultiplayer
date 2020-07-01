using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Systems.Lock;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.Systems.AsteroidComet
{
    public class AsteroidCometSystem : System<AsteroidCometSystem>
    {
        #region Fields

        public AsteroidCometEvents AsteroidCometEvents { get; } = new AsteroidCometEvents();
        public bool AsteroidSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(AsteroidCometSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();

            LockEvent.onLockRelease.Add(AsteroidCometEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Add(AsteroidCometEvents.LevelLoaded);
            TrackingEvent.onStartTrackingAsteroidOrComet.Add(AsteroidCometEvents.StartTrackingCometOrAsteroid);
            TrackingEvent.onStopTrackingAsteroidOrComet.Add(AsteroidCometEvents.StopTrackingCometOrAsteroid);
            GameEvents.onNewVesselCreated.Add(AsteroidCometEvents.NewVesselCreated);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            LockEvent.onLockRelease.Remove(AsteroidCometEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Remove(AsteroidCometEvents.LevelLoaded);
            TrackingEvent.onStartTrackingAsteroidOrComet.Remove(AsteroidCometEvents.StartTrackingCometOrAsteroid);
            TrackingEvent.onStopTrackingAsteroidOrComet.Remove(AsteroidCometEvents.StopTrackingCometOrAsteroid);
            GameEvents.onNewVesselCreated.Remove(AsteroidCometEvents.NewVesselCreated);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Try to acquire the asteroid-spawning lock if nobody else has it.
        /// </summary>
        public void TryGetCometAsteroidLock()
        {
            if (!LockSystem.LockQuery.AsteroidCometLockExists())
                LockSystem.Singleton.AcquireAsteroidLock();
        }

        public int GetAsteroidCount()
        {
            return FlightGlobals.Vessels.Count(v => v.IsAsteroid());
        }

        public int GetCometCount()
        {
            return FlightGlobals.Vessels.Count(v => v.IsComet());
        }

        #endregion
    }
}
