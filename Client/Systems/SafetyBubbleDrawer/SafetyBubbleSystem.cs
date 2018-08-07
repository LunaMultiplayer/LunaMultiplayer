using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.SafetyBubbleDrawer
{
    /// <summary>
    /// This class controls what happens when we are in KSC screen
    /// </summary>
    public class SafetyBubbleSystem : System<SafetyBubbleSystem>
    {
        #region Fields and properties

        public Dictionary<string, List<SpawnPointLocation>> SpawnPoints { get; } = new Dictionary<string, List<SpawnPointLocation>>();

        private static readonly List<GameObject> RendererObjects = new List<GameObject>();

        public SafetyBubbleEvents SafetyBubbleEvents { get; } = new SafetyBubbleEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(SafetyBubbleSystem);

        protected override void OnEnabled()
        {
            FillUpPositions();
            GameEvents.onGameSceneLoadRequested.Add(SafetyBubbleEvents.SwitchScene);
            GameEvents.onFlightReady.Add(SafetyBubbleEvents.FlightReady);
        }

        protected override void OnDisabled()
        {
            SpawnPoints.Clear();
            GameEvents.onGameSceneLoadRequested.Remove(SafetyBubbleEvents.SwitchScene);
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
                return IsInSafetyBubble(vessel.protoVessel.latitude, vessel.protoVessel.longitude,
                    vessel.protoVessel.altitude, vessel.mainBody);

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

            foreach (var point in SpawnPoints[body.name])
            {
                if (Vector3d.Distance(position, point.Position) < SettingsSystem.ServerSettings.SafetyBubbleDistance)
                    return true;
            }

            return false;
        }

        #endregion

        #region Private methods

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

        public static void DrawCircle(GameObject container, float radius, float lineWidth)
        {
            var segments = 360;
            var line = container.GetComponent<LineRenderer>() ?? container.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = Color.blue;
            line.endColor = Color.blue;
            line.positionCount = segments + 1;

            var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
            var points = new Vector3[pointCount];

            for (var i = 0; i < pointCount; i++)
            {
                var rad = Mathf.Deg2Rad * (i * 360f / segments);
                points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
            }

            line.SetPositions(points);
        }

        private void FlightReady()
        {
            foreach (var launchsite in PSystemSetup.Instance.SpaceCenterFacilityLaunchSites)
            {
                foreach (var spawnPoint in launchsite.spawnPoints)
                {
                    DrawCircleAround(spawnPoint.GetSpawnPointTransform(), CreateRenderer());
                }
            }

            foreach (var launchsite in PSystemSetup.Instance.StockLaunchSites)
            {
                foreach (var spawnPoint in launchsite.spawnPoints)
                {
                    DrawCircleAround(spawnPoint.GetSpawnPointTransform(), CreateRenderer());
                }
            }
        }

        private void SwitchScene(GameScenes data)
        {
            if (data != GameScenes.FLIGHT && RendererObjects.Any())
            {
                RendererObjects.ForEach(Object.Destroy);
                RendererObjects.Clear();
            }
        }

        private static LineRenderer CreateRenderer()
        {
            var obj = new GameObject();
            obj.transform.position = Vector3.zero;

            var lineRenderer = obj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.positionCount = (int)((2.0f * Mathf.PI) / 0.01f + 1);
            lineRenderer.useWorldSpace = false;

            RendererObjects.Add(obj);

            return lineRenderer;
        }

        private static void DrawCircleAround(Transform transform, LineRenderer lineRenderer)
        {
            var theta = 0f;
            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                theta += (2.0f * Mathf.PI * 0.01f);
                var x = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Cos(theta);
                var y = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Sin(theta);
                x += transform.position.x;
                y += transform.position.y;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }

            lineRenderer.transform.rotation = Quaternion.Inverse(transform.rotation);
        }
    }
}
