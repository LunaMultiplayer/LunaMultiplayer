using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Systems.SafetyBubble
{
    public class SafetyBubbleEvents : SubSystem<SafetyBubbleSystem>
    {
        private static void DrawCircleAround(Vector3d center, LineRenderer lineRenderer)
        {
            var theta = 0f;
            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                theta += (2.0f * Mathf.PI * 0.01f);
                var x = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Cos(theta);
                var y = SettingsSystem.ServerSettings.SafetyBubbleDistance * Mathf.Sin(theta);
                x += (float)center.x;
                y += (float)center.y;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
        
        public void LeftSafetyBubble()
        {
            if (System.SafetyBubbleObject != null) Object.Destroy(System.SafetyBubbleObject);
        }

        public void EnteredSafetyBubble(SpawnPointLocation spawnPoint)
        {
            if (System.SafetyBubbleObject != null) Object.Destroy(System.SafetyBubbleObject);
            if (spawnPoint == null) return;

            System.SafetyBubbleObject = new GameObject();
            System.SafetyBubbleObject.transform.position = new Vector3((float)spawnPoint.Position.x, (float)spawnPoint.Position.y, (float)spawnPoint.Position.z + 1.5f);
            System.SafetyBubbleObject.transform.rotation = Quaternion.LookRotation(spawnPoint.Body.GetSurfaceNVector(spawnPoint.Latitude, spawnPoint.Longitude));

            var lineRenderer = System.SafetyBubbleObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.positionCount = (int)((2.0f * Mathf.PI) / 0.01f + 1);
            lineRenderer.useWorldSpace = false;

            DrawCircleAround(spawnPoint.Position, lineRenderer);
        }
    }
}
