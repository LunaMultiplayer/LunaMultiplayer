using LunaClient.Base;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalEvents: SubSystem<KerbalSystem>
    {
        public void CrewRemove(ProtoCrewMember protoCrew, int crewCount)
        {
            System.MessageSender.SendKerbalRemove(protoCrew.name);
        }

        public void CrewAdd(ProtoCrewMember protoCrew, int crewCount)
        {
            System.MessageSender.SendKerbal(protoCrew);
        }

        public void CrewSetAsDead(ProtoCrewMember protoCrew, int crewCount)
        {
            protoCrew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
            System.MessageSender.SendKerbal(protoCrew);
        }

        public void FlightReady()
        {
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null)
                return;

            System.ProcessKerbalsInVessel(FlightGlobals.ActiveVessel);
        }

        public void OnCrewKilled(EventReport data)
        {
            var deadKerbalName = data.sender;
            if (HighLogic.CurrentGame.CrewRoster.Exists(deadKerbalName))
            {
                System.MessageSender.SendKerbal(HighLogic.CurrentGame.CrewRoster[deadKerbalName]);
            }
        }
    }
}
