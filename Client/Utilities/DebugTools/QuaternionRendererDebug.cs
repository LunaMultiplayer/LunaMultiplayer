using UnityEngine;

namespace LunaClient.DebugTools
{
    public class QuaternionRendererDebug
    {
        private LineRendererDebug _axis;
        private LineRendererDebug _reference0;
        private LineRendererDebug _theta;

        public QuaternionRendererDebug(Color lineColour)
        {
            _axis = new LineRendererDebug(lineColour);
            _reference0 = new LineRendererDebug(lineColour);
            _theta = new LineRendererDebug(lineColour);
        }

        public void UpdateRotation(Vector3 startPos, Quaternion referenceTransform, Quaternion inputQuat)
        {
            var vec3 = new Vector3(3f, 3f, 3f);
            var vec15 = new Vector3(1.5f, 1.5f, 1.5f);
            var vec05 = new Vector3(0.5f, 0.5f, 0.5f);
            var vec025 = new Vector3(0.25f, 0.25f, 0.25f);

            float inputTheta;
            Vector3 inputAxis;
            inputQuat.ToAngleAxis(out inputTheta, out inputAxis);

            //Theta stuff
            var thetaStart = startPos + Vector3.Scale(referenceTransform*inputAxis, vec15);
            var refDirection = Vector3.up;
            var thetaX = Mathf.Sin(inputTheta*Mathf.Deg2Rad);
            var thetaY = Mathf.Cos(inputTheta*Mathf.Deg2Rad);
            var thetaDirection = new Vector3(thetaX, thetaY, 0f);

            _axis.UpdatePosition(startPos, startPos + Vector3.Scale(referenceTransform*inputAxis, vec3));
            _reference0.UpdatePosition(thetaStart, thetaStart + Vector3.Scale(referenceTransform*refDirection, vec025));
            _theta.UpdatePosition(thetaStart, thetaStart + Vector3.Scale(referenceTransform*thetaDirection, vec05));
        }

        public void Destroy()
        {
            _axis.Destroy();
            _axis = null;
            _reference0.Destroy();
            _reference0 = null;
            _theta.Destroy();
            _theta = null;
        }
    }
}