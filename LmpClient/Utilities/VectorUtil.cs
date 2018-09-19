namespace LmpClient.Utilities
{
    public class VectorUtil
    {
        public static Vector3d LerpUnclamped(Vector3d from, Vector3d to, float t)
        {
            return new Vector3d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
        }
    }
}
