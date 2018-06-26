using UnityEngine;

namespace LunaClient.Extensions
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
                if (vessel.rootPart.Rigidbody?.constraints == RigidbodyConstraints.None)
                    vessel.parts?.ForEach(p => p.Rigidbody.constraints = RigidbodyConstraints.FreezeAll);
            }
        }

        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static void UnfreezePosition(this Vessel vessel)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                if (vessel.rootPart.Rigidbody?.constraints == RigidbodyConstraints.FreezeAll)
                    vessel.parts?.ForEach(p => p.Rigidbody.constraints = RigidbodyConstraints.None);
            }
        }
    }
}
