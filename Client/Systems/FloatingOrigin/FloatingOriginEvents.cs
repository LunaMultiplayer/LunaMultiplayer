using LunaClient.Base;

namespace LunaClient.Systems.FloatingOrigin
{
    public class FloatingOriginEvents : SubSystem<FloatingOriginSystem>
    {
        public void FloatingOriginShift(Vector3d offset, Vector3d offsetNonKrakensbane)
        {
            FloatingOriginSystem.Offset = offset;
            FloatingOriginSystem.OffsetNonKrakensbane = offsetNonKrakensbane;

            if (FlightGlobals.currentMainBody == null)
            {
                FloatingOriginSystem.OffsetLatLonAlt = new double[3];
                FloatingOriginSystem.OffsetNonKrakensbaneLatLonAlt = new double[3];
            }
            else
            {
                FlightGlobals.currentMainBody.GetLatLonAlt(offset, out FloatingOriginSystem.OffsetLatLonAlt[0],
                    out FloatingOriginSystem.OffsetLatLonAlt[1], out FloatingOriginSystem.OffsetLatLonAlt[2]);

                FlightGlobals.currentMainBody.GetLatLonAlt(offsetNonKrakensbane, out FloatingOriginSystem.OffsetNonKrakensbaneLatLonAlt[0],
                    out FloatingOriginSystem.OffsetNonKrakensbaneLatLonAlt[1], out FloatingOriginSystem.OffsetNonKrakensbaneLatLonAlt[2]);
            }
        }
    }
}
