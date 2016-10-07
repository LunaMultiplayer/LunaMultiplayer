using UnityEngine;

namespace LunaClient.DebugTools
{
    public class PointRendererDebug
    {
        private LineRendererDebug _xAxis;
        private LineRendererDebug _yAxis;
        private LineRendererDebug _zAxis;

        public PointRendererDebug(Color lineColour)
        {
            _xAxis = new LineRendererDebug(lineColour);
            _yAxis = new LineRendererDebug(lineColour);
            _zAxis = new LineRendererDebug(lineColour);
        }

        public void UpdatePosition(Vector3 centrePos, Quaternion referenceTransform)
        {
            var xOffset = new Vector3(0.25f, 0f, 0f);
            var yOffset = new Vector3(0f, 0.25f, 0f);
            var zOffset = new Vector3(0f, 0f, 0.25f);
            _xAxis.UpdatePosition(centrePos - referenceTransform*xOffset, centrePos + referenceTransform*xOffset);
            _yAxis.UpdatePosition(centrePos - referenceTransform*yOffset, centrePos + referenceTransform*yOffset);
            _zAxis.UpdatePosition(centrePos - referenceTransform*zOffset, centrePos + referenceTransform*zOffset);
        }

        public void Destroy()
        {
            _xAxis.Destroy();
            _xAxis = null;
            _yAxis.Destroy();
            _yAxis = null;
            _zAxis.Destroy();
            _zAxis = null;
        }
    }
}