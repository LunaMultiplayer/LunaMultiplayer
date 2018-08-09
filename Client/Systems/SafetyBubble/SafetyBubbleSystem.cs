using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.SafetyBubble
{
    /// <inheritdoc />
    /// <summary>
    /// This class controls the code regarding safety bubble
    /// </summary>
    public class SafetyBubbleSystem : System<SafetyBubbleSystem>
    {
        #region Fields and properties

        public GameObject SafetyBubbleObject;
        public GameObject SafetyBubbleObjectX;
        public GameObject SafetyBubbleObjectY;

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
            GameEvents.onFlightReady.Add(SafetyBubbleEvents.FlightReady);
        }

        protected override void OnDisabled()
        {
            _wasInsideSafetyBubble = false;
            SpawnPoints.Clear();
            GameEvents.onFlightReady.Remove(SafetyBubbleEvents.FlightReady);
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
                return IsInSafetyBubble(protoVessel.latitude, protoVessel.longitude, protoVessel.altitude, protoVessel.orbitSnapShot.ReferenceBodyIndex);

            return false;
        }

        /// <summary>
        /// Removes the visual representation of the safety bubble
        /// </summary>
        public void DestroySafetyBubble(float waitSeconds)
        {
            if (SafetyBubbleObject != null) Object.Destroy(SafetyBubbleObject, waitSeconds);
            if (SafetyBubbleObjectX != null) Object.Destroy(SafetyBubbleObjectX, waitSeconds);
            if (SafetyBubbleObjectY != null) Object.Destroy(SafetyBubbleObjectY, waitSeconds);
        }

        public void DrawSafetyBubble()
        {
            DestroySafetyBubble(0);

            var spawnPoint = GetSafetySpawnPoint(FlightGlobals.ActiveVessel);
            if (spawnPoint == null) return;

            SafetyBubbleObject = new GameObject();
            SafetyBubbleObject.transform.position = spawnPoint.Position;
            SafetyBubbleObject.transform.rotation = Quaternion.LookRotation(spawnPoint.Body.GetSurfaceNVector(spawnPoint.Latitude, spawnPoint.Longitude));

            SafetyBubbleObjectX = new GameObject();
            SafetyBubbleObjectX.transform.position = spawnPoint.Position;
            SafetyBubbleObjectX.transform.rotation = SafetyBubbleObject.transform.rotation * Quaternion.Euler(0, 90, 0);

            SafetyBubbleObjectY = new GameObject();
            SafetyBubbleObjectY.transform.position = spawnPoint.Position;
            SafetyBubbleObjectY.transform.rotation = SafetyBubbleObject.transform.rotation * Quaternion.Euler(90, 90, 0);

            DrawCircleAround(spawnPoint.Position, CreateLineRenderer(SafetyBubbleObject));
            DrawCircleAround(spawnPoint.Position, CreateLineRenderer(SafetyBubbleObjectX));
            DrawCircleAround(spawnPoint.Position, CreateLineRenderer(SafetyBubbleObjectY));

            DestroySafetyBubble(10);
        }

        #endregion

        #region Private methods

        private static void DrawCircleAround(Vector3d center, LineRenderer lineRenderer)
        {
            var theta = 0f;
            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                theta += 2.0f * Mathf.PI * 0.01f;
                var x = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Cos(theta);
                var y = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Sin(theta);
                x += (float)center.x;
                y += (float)center.y;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }

        private static LineRenderer CreateLineRenderer(GameObject gameObj)
        {
            var lineRenderer = gameObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.positionCount = (int)(2.0f * Mathf.PI / 0.01f + 1);
            lineRenderer.useWorldSpace = false;
            return lineRenderer;
        }

        private SpawnPointLocation GetSafetySpawnPoint(Vessel vessel)
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

        private bool IsInSafetyBubble(double lat, double lon, double alt, int bodyIndex)
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

        private bool IsInSafetyBubble(double lat, double lon, double alt, CelestialBody body)
        {
            if (body == null)
                return false;

            return IsInSafetyBubble(body.GetWorldSurfacePosition(lat, lon, alt), body);
        }

        private bool IsInSafetyBubble(Vector3d position, CelestialBody body)
        {
            if (!SpawnPoints.ContainsKey(body.name))
                return false;

            foreach (var spawnPoint in SpawnPoints[body.name])
            {
                var distance = Vector3d.Distance(position, spawnPoint.Position);
                if (distance < SettingsSystem.ServerSettings.SafetyBubbleDistance)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
