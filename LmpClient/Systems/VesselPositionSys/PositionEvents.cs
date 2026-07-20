using LmpClient.Base;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.VesselPositionSys
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

        /// <summary>
        /// Broadcast orbit + body name immediately when taking control so the server/TUI can fill ORBIT/body without waiting for the timed position interval.
        /// </summary>
        public void OnVesselSwitching(Vessel fromVessel, Vessel toVessel)
        {
            if (!System.Enabled || VesselCommon.IsSpectating || toVessel == null)
                return;

            if (HighLogic.LoadedScene != GameScenes.FLIGHT || !FlightGlobals.ready)
                return;

            if (toVessel.state == Vessel.State.DEAD || toVessel.vesselType == VesselType.Flag)
                return;

            System.MessageSender.SendVesselPositionUpdate(toVessel, doOrbitDriverReadyCheck: true);
        }
    }
}
