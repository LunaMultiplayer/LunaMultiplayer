using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.KerbalReassigner
{
    public class KerbalReassignerEvents : SubSystem<KerbalReassignerSystem>
    {
        public void OnVesselCreate(Vessel vessel)
        {
            //Kerbals are put in the vessel *after* OnVesselCreate. Thanks squad!.
            if (System.VesselToKerbal.ContainsKey(vessel.id))
                OnVesselDestroyed(vessel);
            if (vessel.GetCrewCount() > 0)
            {
                System.VesselToKerbal.Add(vessel.id, new List<string>());
                foreach (var protoCrewMember in vessel.GetVesselCrew())
                {
                    System.VesselToKerbal[vessel.id].Add(protoCrewMember.name);
                    if (System.KerbalToVessel.ContainsKey(protoCrewMember.name) &&
                        (System.KerbalToVessel[protoCrewMember.name] != vessel.id))
                        Debug.Log("Warning, kerbal double take on " + vessel.id + " ( " + vessel.name + " )");
                    System.KerbalToVessel[protoCrewMember.name] = vessel.id;
                    Debug.Log("OVC " + protoCrewMember.name + " belongs to " + vessel.id);
                }
            }
        }

        public void OnVesselWasModified(Vessel vessel)
        {
            OnVesselDestroyed(vessel);
            OnVesselCreate(vessel);
        }

        public void OnVesselDestroyed(Vessel vessel)
        {
            if (System.VesselToKerbal.ContainsKey(vessel.id))
            {
                foreach (var kerbalName in System.VesselToKerbal[vessel.id])
                    System.KerbalToVessel.Remove(kerbalName);
                System.VesselToKerbal.Remove(vessel.id);
            }
        }

        //Squad workaround - kerbals are assigned after vessel creation for new vessels.
        public void OnFlightReady()
        {
            if (!System.VesselToKerbal.ContainsKey(FlightGlobals.ActiveVessel.id))
                OnVesselCreate(FlightGlobals.ActiveVessel);
        }
    }
}