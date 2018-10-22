using LmpClient.Events;
using UnityEngine;

namespace LmpClient.Extensions
{
    public static class VesselExtension
    {
        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static void FreezePosition(this Vessel vessel)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                if (vessel.rootPart && vessel.rootPart.Rigidbody && vessel.rootPart.Rigidbody.constraints == RigidbodyConstraints.None)
                {
                    vessel.parts?.ForEach(p => p.Rigidbody.constraints = RigidbodyConstraints.FreezeAll);
                }
            }
        }

        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static void UnfreezePosition(this Vessel vessel)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                if (vessel.rootPart && vessel.rootPart.Rigidbody && vessel.rootPart.Rigidbody.constraints == RigidbodyConstraints.FreezeAll)
                {
                    vessel.parts?.ForEach(p => p.Rigidbody.constraints = RigidbodyConstraints.None);
                }
            }
        }


        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static Part FindPart(this Vessel vessel, uint partFlightId)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                for (var i = 0; i < vessel.parts.Count; i++)
                {
                    if (vessel.parts[i].flightID == partFlightId)
                        return vessel.parts[i];
                }
            }

            return null;
        }
        
        public static void RemoveAllCrew(this Vessel vessel)
        {
            foreach (var part in vessel.Parts)
            {
                foreach (var crew in part.protoModuleCrew)
                {
                    vessel.RemoveCrew(crew);
                }
            }

            foreach (var crew in vessel.GetVesselCrew())
            {
                vessel.RemoveCrew(crew);
            }
        }

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        public static void SetImmortal(this Vessel vessel, bool immortal)
        {
            if (vessel == null) return;

            if (vessel.rootPart)
            {
                var isCurrentlyImmortal = float.IsPositiveInfinity(vessel.rootPart.crashTolerance);
                if (isCurrentlyImmortal == immortal) return;
            }

            LunaLog.Log($"Making vessel {vessel.id} {(immortal ? "immortal" : "mortal")}");

            foreach (var part in vessel.Parts)
            {
                part.SetImmortal(immortal);
            }

            var buoyancy = vessel.GetComponent<PartBuoyancy>();
            if (buoyancy)
            {
                buoyancy.enabled = !immortal;
            }

            var collisionEnhancer = vessel.GetComponent<CollisionEnhancer>();
            if (collisionEnhancer)
            {
                collisionEnhancer.enabled = !immortal;
            }

            var flightIntegrator = vessel.GetComponent<FlightIntegrator>();
            if (flightIntegrator)
            {
                flightIntegrator.enabled = !immortal;
            }

            if (immortal)
            {
                ImmortalEvent.onVesselImmortal.Fire(vessel);
            }
            else
            {
                ImmortalEvent.onVesselMortal.Fire(vessel);
            }
        }

    }
}
