using LunaClient.Base;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalEvents: SubSystem<KerbalSystem>
    {
        public void CrewRemove(ProtoCrewMember protoCrew, int crewCount)
        {
            protoCrew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
            System.MessageSender.SendKerbalIfDifferent(protoCrew);
        }

        public void CrewAdd(ProtoCrewMember protoCrew, int crewCount)
        {
            System.MessageSender.SendKerbalIfDifferent(protoCrew);
        }

        public void FlightReady()
        {
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null)
                return;

            System.ProcessKerbalsInVessel(FlightGlobals.ActiveVessel);
        }
    }
}
