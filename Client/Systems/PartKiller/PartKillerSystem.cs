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

        private void RegisterGameHooks()
        {
            GameEvents.onVesselCreate.Add(PartKillerEvents.OnVesselCreate);
            GameEvents.onVesselWasModified.Add(PartKillerEvents.OnVesselWasModified);
            GameEvents.onVesselDestroy.Add(PartKillerEvents.OnVesselDestroyed);
            GameEvents.onFlightReady.Add(PartKillerEvents.OnFlightReady);
        }

        private void UnregisterGameHooks()
        {
            GameEvents.onVesselCreate.Remove(PartKillerEvents.OnVesselCreate);
            GameEvents.onVesselWasModified.Remove(PartKillerEvents.OnVesselWasModified);
            GameEvents.onVesselDestroy.Remove(PartKillerEvents.OnVesselDestroyed);
            GameEvents.onFlightReady.Remove(PartKillerEvents.OnFlightReady);
        }

        #region Fields

        private bool _enabled;
        //Bidictionary
        public Dictionary<Vessel, List<uint>> VesselToPart { get; } = new Dictionary<Vessel, List<uint>>();
        public Dictionary<uint, List<Vessel>> PartToVessel { get; } = new Dictionary<uint, List<Vessel>>();
        public Guid LoadGuid { get; set; } = Guid.Empty;
        private PartKillerEvents PartKillerEvents { get; } = new PartKillerEvents();

        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                    RegisterGameHooks();
                else if (_enabled && !value)
                    UnregisterGameHooks();

                _enabled = value;
            }
        }

        #endregion
    }
}