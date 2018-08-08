using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.SafetyBubbleDrawer
{
    /// <summary>
    /// This class controls the code regarding safety bubble
    /// </summary>
    public class SafetyBubbleSystem : System<SafetyBubbleSystem>
    {
        #region Fields and properties

        public GameObject SafetyBubbleObject;

        public Dictionary<string, List<SpawnPointLocation>> SpawnPoints { get; } = new Dictionary<string, List<SpawnPointLocation>>();

        public SafetyBubbleEvents SafetyBubbleEvents { get; } = new SafetyBubbleEvents();

        private static bool _wasInsideSafetyBubble = false;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(SafetyBubbleSystem);

        protected override void OnEnabled()
        {
            _wasInsideSafetyBubble = false;
            FillUpPositions();
            SetupRoutine(new RoutineDefinition(250, RoutineExecution.Update, FireSafetyBubbleEvents));
            SafetyBubbleEvent.onEnteringSafetyBubble.Add(SafetyBubbleEvents.EnteredSafetyBubble);
            SafetyBubbleEvent.onLeavingSafetyBubble.Add(SafetyBubbleEvents.LeftSafetyBubble);
        }

        protected override void OnDisabled()
        {
            _wasInsideSafetyBubble = false;
            SpawnPoints.Clear();
            SafetyBubbleEvent.onEnteringSafetyBubble.Remove(SafetyBubbleEvents.EnteredSafetyBubble);
            SafetyBubbleEvent.onLeavingSafetyBubble.Remove(SafetyBubbleEvents.LeftSafetyBubble);
        }

        #endregion

        #region Update routines

        private void FireSafetyBubbleEvents()
        {
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null)
            {
                _wasInsideSafetyBubble = false;
                return;
            }

            var inSafetyBubble = IsInSafetyBubble(FlightGlobals.ActiveVessel, false);
            if (inSafetyBubble && !_wasInsideSafetyBubble)
            {
                _wasInsideSafetyBubble = true;
                SafetyBubbleEvent.onEnteringSafetyBubble.Fire(GetSafetySpawnPoint(FlightGlobals.ActiveVessel));
            }
            else if (!inSafetyBubble && _wasInsideSafetyBubble)
            {
                _wasInsideSafetyBubble = false;
                SafetyBubbleEvent.onLeavingSafetyBubble.Fire();
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Returns whether the given vessel is in a starting safety bubble or not.
        /// </summary>
        public bool IsInSafetyBubble(Vessel vessel, bool useLatLonAltFromProto = true)
        {
            if (vessel == null)
                return false;

            if (vessel.situation >= Vessel.Situations.FLYING)
                return false;

            if (SettingsSystem.ServerSettings.SafetyBubbleDistance <= 0)
                return false;

            if (useLatLonAltFromProto)
                //Use the protovessel values as the normal vessel values can be affected by the position system and the situation of the vessel
                return IsInSafetyBubble(vessel.protoVessel.latitude, vessel.protoVessel.longitude, vessel.protoVessel.altitude, vessel.mainBody);

            return IsInSafetyBubble(vessel.latitude, vessel.longitude, vessel.altitude, vessel.mainBody);
        }

        /// <summary>
        /// Returns whether the given protovessel is in a starting safety bubble or not.
        /// </summary>
        public bool IsInSafetyBubble(ProtoVessel protoVessel)
        {
            if (protoVessel == null)
                return true;

            if (protoVessel.orbitSnapShot != null)
                return IsInSafetyBubble(protoVessel.latitude, protoVessel.longitude, protoVessel.altitude,
                    protoVessel.orbitSnapShot.ReferenceBodyIndex);

            return false;
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public bool IsInSafetyBubble(double lat, double lon, double alt, int bodyIndex)
        {
            if (bodyIndex < FlightGlobals.Bodies.Count)
            {
                var body = FlightGlobals.Bodies[bodyIndex];
                if (body == null)
                    return false;

                return IsInSafetyBubble(FlightGlobals.Bodies[bodyIndex].GetWorldSurfacePosition(lat, lon, alt), body);
            }

            LunaLog.LogError($"Body index {bodyIndex} is out of range!");
            return false;
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public bool IsInSafetyBubble(double lat, double lon, double alt, CelestialBody body)
        {
            if (body == null)
                return false;

            return IsInSafetyBubble(body.GetWorldSurfacePosition(lat, lon, alt), body);
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public bool IsInSafetyBubble(Vector3d position, CelestialBody body)
        {
            if (!SpawnPoints.ContainsKey(body.name))
                return false;

            foreach (var spawnPoint in SpawnPoints[body.name])
            {
                if (Vector2d.Distance(new Vector2d(position.x, position.y), new Vector2d(spawnPoint.Position.x, spawnPoint.Position.y)) < SettingsSystem.ServerSettings.SafetyBubbleDistance)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Private methods

        public SpawnPointLocation GetSafetySpawnPoint(Vessel vessel)
        {
            foreach (var point in SpawnPoints[vessel.mainBody.name])
            {
                if (Vector3d.Distance(vessel.vesselTransform.position, point.Position) < SettingsSystem.ServerSettings.SafetyBubbleDistance)
                {
                    return point;
                }
            }

            return null;
        }

        private void FillUpPositions()
        {
            foreach (var launchsite in PSystemSetup.Instance.SpaceCenterFacilityLaunchSites)
            {
                if (!SpawnPoints.ContainsKey(launchsite.hostBody.name))
                    SpawnPoints.Add(launchsite.hostBody.name, new List<SpawnPointLocation>());

                foreach (var spawnPoint in launchsite.spawnPoints)
                {
                    SpawnPoints[launchsite.hostBody.name].Add(new SpawnPointLocation(spawnPoint, launchsite.hostBody));
                }
            }

            foreach (var launchsite in PSystemSetup.Instance.StockLaunchSites)
            {
                if (!SpawnPoints.ContainsKey(launchsite.Body.name))
                    SpawnPoints.Add(launchsite.Body.name, new List<SpawnPointLocation>());

                foreach (var spawnPoint in launchsite.spawnPoints)
                {
                    SpawnPoints[launchsite.Body.name].Add(new SpawnPointLocation(spawnPoint, launchsite.Body));
                }
            }
        }

        #endregion
    }
}
