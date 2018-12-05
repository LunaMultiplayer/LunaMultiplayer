using LmpClient.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// This is a proof of concept. A class that can reload vessel parts
    /// </summary>
    public class OwnVesselReloader
    {
        public static bool ReloadOwnVessel(ProtoVessel protoVessel)
        {
            var partIdsToRemove = FlightGlobals.ActiveVessel.parts.Select(p => p.flightID)
                .Except(protoVessel.protoPartSnapshots.Select(pp => pp.flightID)).ToArray();

            foreach (var partIdToRemove in partIdsToRemove)
            {
                FlightGlobals.ActiveVessel.FindPart(partIdToRemove)?.Die();
            }

            var partIdsToCreate = protoVessel.protoPartSnapshots.Select(pp => pp.flightID)
                .Except(FlightGlobals.ActiveVessel.parts.Select(p => p.flightID)).ToArray();

            var partsToInit = new List<ProtoPartSnapshot>();
            foreach (var partIdToCreate in partIdsToCreate)
            {
                var newProtoPart = protoVessel.GetProtoPart(partIdToCreate);
                var newPart = newProtoPart.Load(FlightGlobals.ActiveVessel, false);
                FlightGlobals.ActiveVessel.parts.Add(newPart);
                partsToInit.Add(newProtoPart);
            }

            //Init new parts. This must be done in another loop as otherwise new parts won't have their correct attachment parts.
            foreach (var partSnapshot in partsToInit)
                partSnapshot.Init(FlightGlobals.ActiveVessel);

            var crewedParts = FlightGlobals.ActiveVessel.parts.Where(p => p.protoModuleCrew.Any())
                .Select(p => new { p.protoModuleCrew.Count, p.flightID, p, p.protoModuleCrew }).ToArray();
            var crewedProtoParts = protoVessel.protoPartSnapshots.Where(p => p.protoModuleCrew.Any())
                .Select(p => new { p.protoModuleCrew.Count, p.flightID, p, p.protoModuleCrew }).ToArray();

            foreach (var crewedPart in crewedParts)
            {
                var crewedProtoPart = crewedProtoParts.FirstOrDefault(pp => pp.flightID == crewedPart.flightID);
                if (crewedProtoPart != null)
                {
                    if (crewedProtoPart.Count > crewedPart.Count)
                    {
                        var crewToAdd = crewedProtoPart.protoModuleCrew.Select(c => c.name)
                            .Except(crewedPart.protoModuleCrew.Select(c => c.name)).ToArray();

                        foreach (var crewMember in crewToAdd)
                        {
                            crewedPart.p.AddCrew(crewedProtoPart.protoModuleCrew.First(m => m.name == crewMember));
                        }
                    }
                    else if (crewedProtoPart.Count < crewedPart.Count)
                    {
                        var crewToRemove = crewedPart.protoModuleCrew.Select(c => c.name)
                            .Except(crewedProtoPart.protoModuleCrew.Select(c => c.name)).ToArray();

                        foreach (var crewMember in crewToRemove)
                        {
                            crewedPart.p.RemoveCrew(crewedPart.protoModuleCrew.First(m => m.name == crewMember));
                        }
                    }
                }
            }

            FlightGlobals.ActiveVessel.RebuildCrewList();

            return true;
        }
    }
}
