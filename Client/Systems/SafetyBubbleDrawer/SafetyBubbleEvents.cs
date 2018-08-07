using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.SafetyBubbleDrawer
{
    public class SafetyBubbleEvents : SubSystem<SafetyBubbleSystem>
    {
        private static readonly List<GameObject> RendererObjects = new List<GameObject>();

        public void FlightReady()
        {
            foreach (var spawnPoint in System.SpawnPoints.Values.SelectMany(v=> v))
            {
                DrawCircleAround(spawnPoint, CreateRenderer());
            }
        }

        public void SwitchScene(GameScenes data)
        {
            if (data != GameScenes.FLIGHT && RendererObjects.Any())
            {
                RendererObjects.ForEach(Object.Destroy);
                RendererObjects.Clear();
            }
        }

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

        private static void DrawCircleAround(SpawnPointLocation spawnPoint, LineRenderer lineRenderer)
        {
            var theta = 0f;
            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                theta += (2.0f * Mathf.PI * 0.01f);
                var x = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Cos(theta);
                var y = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Sin(theta);
                x += (float)spawnPoint.Position.x;
                y += (float)spawnPoint.Position.y;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }

            lineRenderer.transform.rotation = Quaternion.Inverse(spawnPoint.Transform.rotation);
        }
    }
}
