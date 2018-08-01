using LunaClient.Base;

namespace LunaClient.Systems.FloatingOrigin
{
    public class FloatingOriginEvents : SubSystem<FloatingOriginSystem>
    {
        public void FloatingOriginShift(Vector3d offset, Vector3d offsetNonKrakensbane)
        {
            FloatingOriginSystem.Offset = offset;
            FloatingOriginSystem.OffsetNonKrakensbane = offsetNonKrakensbane;
        }
    }
}
