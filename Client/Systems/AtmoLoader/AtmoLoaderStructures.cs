using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Utilities;

namespace LunaClient.Systems.AtmoLoader
{
    public class FlyingVesselLoad
    {
        public Vessel FlyingVessel { get; set; }
        public VesselUpdate LastVesselUpdate { get; set; }
        public double LastUnpackTime { get; set; }
    }
}