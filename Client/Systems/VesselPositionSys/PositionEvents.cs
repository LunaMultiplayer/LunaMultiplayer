using LunaClient.Base;

namespace LunaClient.Systems.VesselPositionSys
{
    public class PositionEvents : SubSystem<VesselPositionSystem>
    {
        /// <summary>
        /// When stop warping adjust the interpolation times of long running packets
        /// </summary>
        public void WarpStopped()
        {
            System.AdjustExtraInterpolationTimes();
        }
    }
}
