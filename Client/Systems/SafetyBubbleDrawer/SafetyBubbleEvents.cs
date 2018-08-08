using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Systems.SafetyBubbleDrawer
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

        public void EnteredSafetyBubble(Vector3d safetyBubbleCenter)
        {
            if (System.SafetyBubbleObject != null) Object.Destroy(System.SafetyBubbleObject);

            System.SafetyBubbleObject = new GameObject();
            System.SafetyBubbleObject.transform.position = safetyBubbleCenter;
            System.SafetyBubbleObject.transform.rotation = FlightGlobals.currentMainBody.rotation;

            var lineRenderer = System.SafetyBubbleObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startWidth = 0.3f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.positionCount = (int)((2.0f * Mathf.PI) / 0.01f + 1);
            lineRenderer.useWorldSpace = false;

            DrawCircleAround(safetyBubbleCenter, lineRenderer);
        }
    }
}
