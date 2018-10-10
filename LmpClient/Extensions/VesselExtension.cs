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
    }
}
