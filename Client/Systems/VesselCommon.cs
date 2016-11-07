using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;

namespace LunaClient.Systems
{
    public class VesselCommon
    {
        public static Guid CurrentVesselId => FlightGlobals.ActiveVessel == null ? Guid.Empty : FlightGlobals.ActiveVessel.id;

        private static readonly double LANDING_PAD_LATITUDE = -0.0971978130377757;
        private static readonly double LANDING_PAD_LONGITUDE = 285.44237039111;
        private static readonly double RUNWAY_LATITUDE = -0.0486001121594686;
        private static readonly double RUNWAY_LONGITUDE = 285.275552559723;
        private static readonly double KSC_ALTITUDE = 60;
        
        public static bool UpdateIsForOwnVessel(Guid vesselId)
        {
            //Ignore updates to our own vessel if we aren't spectating
            return !IsSpectating &&
                   (FlightGlobals.ActiveVessel != null) &&
                   (FlightGlobals.ActiveVessel.id == vesselId);
        }

        public static bool ActiveVesselIsInSafetyBubble()
        {
            return IsInSafetyBubble(FlightGlobals.ActiveVessel);
        }

        private static bool _isSpectating;
        public static bool IsSpectating
        {
            get
            {
                return HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && _isSpectating;
            }
            set
            {
                _isSpectating = value;
            }
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
                if (LockSystem.Singleton.SpectatorLockExists(FlightGlobals.ActiveVessel.id))
                    return true;

                var controlledVesselsIds = LockSystem.Singleton.GetLocksWithPrefix("control-").Select(v => new Guid(v.Substring(8)));
                return FlightGlobals.VesselsLoaded.Where(v => v.id != FlightGlobals.ActiveVessel.id).Any(v => controlledVesselsIds.Contains(v.id));
            }

            return false;
        }

        /// <summary>
        /// Return all the vessels except the active one that we have the update lock
        /// </summary>
        public static IEnumerable<Vessel> GetSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly

            return LockSystem.Singleton.GetPlayerLocksPrefix(SettingsSystem.CurrentSettings.PlayerName, "update-")
                .Select(l => new Guid(l.Substring(7)))
                .Where(vi => vi != FlightGlobals.ActiveVessel.id)
                .Select(vi => FlightGlobals.VesselsLoaded.FirstOrDefault(v => v.id == vi))
                .Where(v => v != null)
                .ToArray();
        }

        /// <summary>
        /// Return all the abandoned vessels (evssels that are not loaded and don't have update lock
        /// </summary>
        public static IEnumerable<Vessel> GetAbandonedVessels()
        {
            return FlightGlobals.VesselsUnloaded.Where(v => !LockSystem.Singleton.LockExists("update-" + v.id));
        }

        /// <summary>
        /// Returns if given vessel is controlled and in a past subspace
        /// </summary>
        public static bool VesselIsControlledAndInPastSubspace(Guid vesselId)
        {
            var owner = "";
            if (LockSystem.Singleton.LockExists("control-" + vesselId))
            {
                owner = LockSystem.Singleton.LockOwner("control-" + vesselId);
            }
            else if (LockSystem.Singleton.LockExists("update-" + vesselId))
            {
                owner = LockSystem.Singleton.LockOwner("update-" + vesselId);
            }
            
            return !string.IsNullOrEmpty(owner) && WarpSystem.Singleton.PlayerIsInPastSubspace(owner);
        }

        /// <summary>
        /// Returns whether the active vessel is near KSC or not.  Measures distance from the landing pad, so very small values may not cover all of KSC.
        /// </summary>
        /// <param name="distance">The distance to compare</param>
        /// <returns></returns>
        public static bool isNearKSC(int distance)
        {
            return isNearKSC(FlightGlobals.ActiveVessel, distance);
        }

        /// <summary>
        /// Returns whether the given vessel is near KSC or not.  Measures distance from the landing pad, so very small values may not cover all of KSC.
        /// </summary>
        /// <param name="vessel">The vessel used to determine the distance.  If null, the vessel is not near KSC.</param>
        /// <param name="distance">The distance to compare</param>
        /// <returns></returns>
        public static bool isNearKSC(Vessel vessel, int distance)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel == null || vessel.mainBody.name != "Kerbin")
                return false;
            var landingPadPosition = vessel.mainBody.GetWorldSurfacePosition(LANDING_PAD_LATITUDE, LANDING_PAD_LONGITUDE, KSC_ALTITUDE);
            var landingPadDistance = Vector3d.Distance(vessel.GetWorldPos3D(), landingPadPosition);
            return (landingPadDistance < distance);
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
            var landingPadPosition = vessel.mainBody.GetWorldSurfacePosition(LANDING_PAD_LATITUDE, LANDING_PAD_LONGITUDE, KSC_ALTITUDE);
            var runwayPosition = vessel.mainBody.GetWorldSurfacePosition(RUNWAY_LATITUDE, RUNWAY_LONGITUDE, KSC_ALTITUDE);
            var landingPadDistance = Vector3d.Distance(vessel.GetWorldPos3D(), landingPadPosition);
            var runwayDistance = Vector3d.Distance(vessel.GetWorldPos3D(), runwayPosition);
            return (runwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance) ||
                (landingPadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance);
        }

        public static bool IsInSafetyBubble(Vector3d worlPos, CelestialBody body)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (body.name != "Kerbin")
                return false;
            var landingPadPosition = body.GetWorldSurfacePosition(LANDING_PAD_LATITUDE, LANDING_PAD_LONGITUDE, KSC_ALTITUDE);
            var runwayPosition = body.GetWorldSurfacePosition(RUNWAY_LATITUDE, RUNWAY_LONGITUDE, KSC_ALTITUDE);
            var landingPadDistance = Vector3d.Distance(worlPos, landingPadPosition);
            var runwayDistance = Vector3d.Distance(worlPos, runwayPosition);
            return (runwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance) ||
                (landingPadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance);
        }

        public static bool EnableAllSystems
        {
            set
            {
                if (value)
                {
                    VesselLockSystem.Singleton.Enabled = true;
                    VesselUpdateSystem.Singleton.Enabled = true;
                    VesselChangeSystem.Singleton.Enabled = true;
                    VesselProtoSystem.Singleton.Enabled = true;
                    VesselRemoveSystem.Singleton.Enabled = true;
                    VesselDockSystem.Singleton.Enabled = true;
                }
                else
                {
                    VesselLockSystem.Singleton.Enabled = false;
                    VesselUpdateSystem.Singleton.Enabled = false;
                    VesselChangeSystem.Singleton.Enabled = false;
                    VesselProtoSystem.Singleton.Enabled = false;
                    VesselRemoveSystem.Singleton.Enabled = false;
                    VesselDockSystem.Singleton.Enabled = false;
                }
            }
        }
    }
}
