using LmpClient.Base;

namespace LmpClient.Systems.VesselFlightStateSys
{
    public class FlightStateEvents : SubSystem<VesselFlightStateSystem>
    {
        public void OnVesselPack(Vessel vessel)
        {
            System.RemoveVessel(vessel);
        }

        public void OnVesselUnpack(Vessel vessel)
        {
            System.AddVesselToSystem(vessel);
        }

        public void OnStartSpectating()
        {
            System.AddVesselToSystem(FlightGlobals.ActiveVessel);
        }

        public void OnFinishedSpectating()
        {
            System.RemoveVessel(FlightGlobals.ActiveVessel);
        }

        /// <summary>
        /// When stop warping adjust the interpolation times of long running packets
        /// </summary>
        public void WarpStopped()
        {
            System.AdjustExtraInterpolationTimes();
        }
    }
}
