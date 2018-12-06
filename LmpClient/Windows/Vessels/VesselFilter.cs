using LmpClient.Extensions;
using LmpClient.Systems.Lock;
using UnityEngine;

namespace LmpClient.Windows.Vessels
{
    public class VesselFilter
    {
        public static bool HideAsteroids = true;
        public static bool HideDebris = true;
        public static bool HideUncontrolled = false;

        public static void DrawFilters()
        {
            GUILayout.BeginHorizontal();
            HideAsteroids = GUILayout.Toggle(HideAsteroids, "Hide asteroids");
            HideDebris = GUILayout.Toggle(HideDebris, "Hide debris");
            HideUncontrolled = GUILayout.Toggle(HideUncontrolled, "Hide uncontrolled");
            GUILayout.EndHorizontal();
        }

        public static bool MatchesFilters(Vessel vessel)
        {
            if (HideAsteroids && vessel.IsAsteroid())
                return false;

            if (HideDebris && vessel.vesselType == VesselType.Debris)
                return false;

            if (HideUncontrolled && !LockSystem.LockQuery.ControlLockExists(vessel.id))
                return false;

            return true;
        }
    }
}
