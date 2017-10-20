using System;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockStructure
    {
        public Guid DominantVesselId { get; set; }
        public Guid WeakVesselId { get; set; }

        public Vessel DominantVessel { get; set; }
        public Vessel WeakVessel { get; set; }

        public VesselDockStructure(Guid vessel1Id, Guid vessel2Id)
        {
            var vessel1 = FlightGlobals.FindVessel(vessel1Id);
            var vessel2 = FlightGlobals.FindVessel(vessel2Id);

            if (vessel1 != null && vessel2 != null)
            {
                DominantVessel = Vessel.GetDominantVessel(vessel1, vessel2);
                DominantVesselId = DominantVessel.id;

                WeakVesselId = DominantVesselId == vessel1Id ? vessel2Id : vessel1Id;
                WeakVessel = DominantVesselId == vessel1Id ? vessel2 : vessel1;
            }
        }

        public bool StructureIsOk()
        {
            return DominantVessel != null && WeakVessel != null;
        }
    }
}
