using System;
using System.Collections.Generic;
using LunaClient.Base;

namespace LunaClient.Systems.PartKiller
{
    public class PartKillerSystem : System<PartKillerSystem>
    {
        public void ForgetVessel(Vessel vessel)
        {
            if (!VesselToPart.ContainsKey(vessel)) return; //Killed as a fragment.

            foreach (var partId in VesselToPart[vessel])
            {
                PartToVessel[partId].Remove(vessel);
                if (PartToVessel[partId].Count == 0)
                    PartToVessel.Remove(partId);
            }
            VesselToPart.Remove(vessel);
        }

        public void AddVesselAndPartsToBiDictionary(Vessel vessel, List<uint> vesselParts)
        {
            VesselToPart.Add(vessel, vesselParts);
            foreach (var partId in vesselParts)
            {
                if (!PartToVessel.ContainsKey(partId))
                    PartToVessel.Add(partId, new List<Vessel>());
                PartToVessel[partId].Add(vessel);
            }
        }

        public override void OnEnabled()
        {
            GameEvents.onVesselCreate.Add(PartKillerEvents.OnVesselCreate);
            GameEvents.onVesselWasModified.Add(PartKillerEvents.OnVesselWasModified);
            GameEvents.onVesselDestroy.Add(PartKillerEvents.OnVesselDestroyed);
            GameEvents.onFlightReady.Add(PartKillerEvents.OnFlightReady);
        }

        public override void OnDisabled()
        {
            GameEvents.onVesselCreate.Remove(PartKillerEvents.OnVesselCreate);
            GameEvents.onVesselWasModified.Remove(PartKillerEvents.OnVesselWasModified);
            GameEvents.onVesselDestroy.Remove(PartKillerEvents.OnVesselDestroyed);
            GameEvents.onFlightReady.Remove(PartKillerEvents.OnFlightReady);
        }

        #region Fields
        
        //Bidictionary
        public Dictionary<Vessel, List<uint>> VesselToPart { get; } = new Dictionary<Vessel, List<uint>>();
        public Dictionary<uint, List<Vessel>> PartToVessel { get; } = new Dictionary<uint, List<Vessel>>();
        public Guid LoadGuid { get; set; } = Guid.Empty;
        private PartKillerEvents PartKillerEvents { get; } = new PartKillerEvents();
        
        #endregion
    }
}