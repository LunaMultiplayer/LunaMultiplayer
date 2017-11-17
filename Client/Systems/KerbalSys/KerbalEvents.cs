using LunaClient.Base;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalEvents: SubSystem<KerbalSystem>
    {
        public void CrewChange(ProtoCrewMember protoCrew, int data1)
        {
            System.MessageSender.SendKerbalIfDifferent(protoCrew);
        }

        public void VesselLoad(Vessel data)
        {
            //We only send crew data if we are NOT spectating and the ship that we are loading is OUR ship
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel?.id != data.id)
                return;

            System.ProcessKerbalsInVessel(data);
        }
    }
}
