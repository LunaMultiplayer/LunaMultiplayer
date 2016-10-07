using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Utilities;

namespace LunaClient.Systems.PartKiller
{
    public class PartKillerEvents : SubSystem<PartKillerSystem>
    {
        /// <summary>
        /// This event is called after game is loaded, vessels initialized and all systems running and ready to fly
        /// </summary>
        public void OnFlightReady()
        {
            if (FlightGlobals.ActiveVessel.id == System.LoadGuid)
            {
                System.LoadGuid = Guid.Empty;
                var protoVessel = FlightGlobals.ActiveVessel.BackupVessel();
                var vesselParts = protoVessel.protoPartSnapshots.Select(pps => pps.flightID).ToList();

                System.AddVesselAndPartsToBiDictionary(FlightGlobals.ActiveVessel, vesselParts);
            }
        }

        /// <summary>
        /// Called when a vessel is spawned into the game
        /// </summary>
        /// <param name="vessel"></param>
        public void OnVesselCreate(Vessel vessel)
        {
            var protoVessel = vessel.BackupVessel();
            var vesselParts = new List<uint>();

            var killShip = false;
            var vesselHasLocks = false;

            //When you spawn a new vessel in all the part ID's are 0 until OnFlightReady.
            if (protoVessel.protoPartSnapshots.Any(pp => pp.flightID == 0))
            {
                System.LoadGuid = vessel.id;
                return;
            }

            foreach (var protoPart in protoVessel.protoPartSnapshots)
            {
                vesselParts.Add(protoPart.flightID);
                killShip = System.PartToVessel.ContainsKey(protoPart.flightID);

                if (killShip)
                    vesselHasLocks = System.PartToVessel[protoPart.flightID].Any(v => (v.id == vessel.id) ||
                        !LockSystem.Singleton.LockExists("control-" + v.id) || LockSystem.Singleton.LockIsOurs("control-" + v.id) || LockSystem.Singleton.LockIsOurs("update-" + v.id));
            }

            if (killShip && !vesselHasLocks)
            {
                LunaLog.Debug("PartKiller: Destroying vessel fragment");
                vessel.Die();
            }
            else
            {
                System.AddVesselAndPartsToBiDictionary(vessel, vesselParts);
            }
        }

        public void OnVesselWasModified(Vessel vessel)
        {
            if (!System.VesselToPart.ContainsKey(vessel)) return; //Killed as a fragment.

            var currentVesselParts = vessel.BackupVessel().protoPartSnapshots.Select(p => p.flightID).ToArray();

            var partsToAdd = currentVesselParts.Except(System.VesselToPart[vessel]).ToArray();
            var partsToRemove = System.VesselToPart[vessel].Except(currentVesselParts).ToArray();

            foreach (var partId in partsToAdd)
            {
                if (!System.PartToVessel.ContainsKey(partId))
                    System.PartToVessel.Add(partId, new List<Vessel>());
                System.PartToVessel[partId].Add(vessel);
            }
            System.VesselToPart[vessel].AddRange(partsToAdd);

            foreach (var partId in partsToRemove)
            {
                System.VesselToPart[vessel].Remove(partId);
                System.PartToVessel[partId].Remove(vessel);
                if (System.PartToVessel[partId].Count == 0)
                    System.PartToVessel.Remove(partId);
            }
        }

        /// <summary>
        /// Called when vessel goes BOOM
        /// </summary>
        public void OnVesselDestroyed(Vessel vessel)
        {
            System.ForgetVessel(vessel);
        }
    }
}