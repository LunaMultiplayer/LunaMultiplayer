using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Systems
{
    /// <summary>
    /// Class to hold common logic regarding the Vessel systems
    /// </summary>
    public class VesselCommon
    {
        public static Guid CurrentVesselId => FlightGlobals.ActiveVessel == null ? Guid.Empty : FlightGlobals.ActiveVessel.id;

        private const double LandingPadLatitude = -0.0971978130377757;
        private const double LandingPadLongitude = 285.44237039111;
        private const double RunwayLatitude = -0.0486001121594686;
        private const double RunwayLongitude = 285.275552559723;
        private const double KscAltitude = 60;

        public static bool UpdateIsForOwnVessel(Guid vesselId)
        {
            //Ignore updates to our own vessel if we aren't spectating
            return !IsSpectating &&
                   FlightGlobals.ActiveVessel != null &&
                   FlightGlobals.ActiveVessel.id == vesselId;
        }

        public static bool ActiveVesselIsInSafetyBubble()
        {
            return IsInSafetyBubble(FlightGlobals.ActiveVessel);
        }

        private static bool _isSpectating;
        public static bool IsSpectating
        {
            get => HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && _isSpectating;
            set => _isSpectating = value;
        }

        /// <summary>
        /// Return the controlled vessels
        /// </summary>
        public static Vessel[] GetControlledVessels()
        {
            return GetControlledVesselIds()
                .Select(FlightGlobals.FindVessel)
                .ToArray();
        }

        /// <summary>
        /// Check if someone is spectating current vessel
        /// </summary>
        /// <returns></returns>
        public static bool IsSomeoneSpectatingUs => !IsSpectating && FlightGlobals.ActiveVessel != null && SystemsContainer.Get<LockSystem>().SpectatorLockExists(FlightGlobals.ActiveVessel.id);


        /// <summary>
        /// Return the controlled vessel ids
        /// </summary>
        public static Guid[] GetControlledVesselIds()
        {
            return SystemsContainer.Get<LockSystem>().GetLocksWithPrefix("control-").Select(v => new Guid(LockSystem.TrimLock(v))).ToArray();
        }

        /// <summary>
        /// Check if there are other player controlled vessels nearby
        /// </summary>
        /// <returns></returns>
        public static bool PlayerVesselsNearby()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                //If there is someone spectating us then return true and update it fast;
                if (IsSomeoneSpectatingUs)
                {
                    return true;
                }

                var controlledVesselsIds = GetControlledVesselIds();
                var loadedVesselIds = FlightGlobals.VesselsLoaded.Where(v => v.id != FlightGlobals.ActiveVessel.id)
                    .Select(v => v.id);

                return controlledVesselsIds.Intersect(loadedVesselIds).Any();
            }

            return false;
        }

        /// <summary>
        /// Return all the vessels except the active one that we have the update lock
        /// </summary>
        public static IEnumerable<Vessel> GetSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly

            return SystemsContainer.Get<LockSystem>().GetPlayerLocksPrefix(SettingsSystem.CurrentSettings.PlayerName, "update-")
                .Select(l => new Guid(LockSystem.TrimLock(l)))
                .Where(vi => vi != FlightGlobals.ActiveVessel.id)
                .Select(vi => FlightGlobals.VesselsLoaded.FirstOrDefault(v => v.id == vi))
                .Where(v => v != null)
                .ToArray();
        }

        /// <summary>
        /// Return all the abandoned vessels (vessels that are not loaded and don't have update lock
        /// </summary>
        public static IEnumerable<Vessel> GetAbandonedVessels()
        {
            return FlightGlobals.VesselsUnloaded.Where(v => !SystemsContainer.Get<LockSystem>().LockExists($"update-{v.id}"));
        }

        /// <summary>
        /// Returns if given vessel is controlled and in a past subspace
        /// </summary>
        public static bool VesselIsControlledAndInPastSubspace(Guid vesselId)
        {
            var owner = "";
            if (SystemsContainer.Get<LockSystem>().LockExists($"control-{vesselId}"))
            {
                owner = SystemsContainer.Get<LockSystem>().LockOwner($"control-{vesselId}");
            }
            else if (SystemsContainer.Get<LockSystem>().LockExists($"update-{vesselId}"))
            {
                owner = SystemsContainer.Get<LockSystem>().LockOwner($"update-{vesselId}");
            }

            return !string.IsNullOrEmpty(owner) && SystemsContainer.Get<WarpSystem>().PlayerIsInPastSubspace(owner);
        }

        /// <summary>
        /// Returns whether the active vessel is near KSC or not.  Measures distance from the landing pad, so very small values may not cover all of KSC.
        /// </summary>
        /// <param name="distance">The distance to compare</param>
        /// <returns></returns>
        public static bool IsNearKsc(int distance)
        {
            return IsNearKsc(FlightGlobals.ActiveVessel, distance);
        }

        /// <summary>
        /// Returns whether the given vessel is in a starting safety bubble or not.
        /// </summary>
        /// <param name="vessel">The vessel used to determine the distance.  If null, the vessel is not in the safety bubble.</param>
        /// <returns></returns>
        public static bool IsInSafetyBubble(Vessel vessel)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel == null || vessel.mainBody.name != "Kerbin")
                return false;

            var landingPadPosition = vessel.mainBody.GetWorldSurfacePosition(LandingPadLatitude, LandingPadLongitude, KscAltitude);
            var landingPadDistance = Vector3d.Distance(vessel.GetWorldPos3D(), landingPadPosition);

            if (landingPadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance) return true;

            //We are far from the pad, let's see if happens the same in the runway...
            var runwayPosition = vessel.mainBody.GetWorldSurfacePosition(RunwayLatitude, RunwayLongitude, KscAltitude);
            var runwayDistance = Vector3d.Distance(vessel.GetWorldPos3D(), runwayPosition);

            return runwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance;
        }

        /// <summary>
        /// Returns whether the given vessel is near KSC or not.  Measures distance from the landing pad, so very small values may not cover all of KSC.
        /// </summary>
        /// <param name="vessel">The vessel used to determine the distance.  If null, the vessel is not near KSC.</param>
        /// <param name="distance">The distance to compare</param>
        private static bool IsNearKsc(Vessel vessel, int distance)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel == null || vessel.mainBody.name != "Kerbin")
                return false;

            var landingPadPosition = vessel.mainBody.GetWorldSurfacePosition(LandingPadLatitude, LandingPadLongitude, KscAltitude);
            var landingPadDistance = Vector3d.Distance(vessel.GetWorldPos3D(), landingPadPosition);

            return landingPadDistance < distance;
        }
    }
}
