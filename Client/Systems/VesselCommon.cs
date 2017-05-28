using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRangeSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
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
        public static bool IsSomeoneSpectatingUs => !IsSpectating && FlightGlobals.ActiveVessel != null && LockSystem.Singleton.SpectatorLockExists(FlightGlobals.ActiveVessel.id);


        /// <summary>
        /// Return the controlled vessel ids
        /// </summary>
        public static Guid[] GetControlledVesselIds()
        {
            return LockSystem.Singleton.GetLocksWithPrefix("control-").Select(v => new Guid(LockSystem.TrimLock(v))).ToArray();
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

            return LockSystem.Singleton.GetPlayerLocksPrefix(SettingsSystem.CurrentSettings.PlayerName, "update-")
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
            return FlightGlobals.VesselsUnloaded.Where(v => !LockSystem.Singleton.LockExists($"update-{v.id}"));
        }

        /// <summary>
        /// Returns if given vessel is controlled and in a past subspace
        /// </summary>
        public static bool VesselIsControlledAndInPastSubspace(Guid vesselId)
        {
            var owner = "";
            if (LockSystem.Singleton.LockExists($"control-{vesselId}"))
            {
                owner = LockSystem.Singleton.LockOwner($"control-{vesselId}");
            }
            else if (LockSystem.Singleton.LockExists($"update-{vesselId}"))
            {
                owner = LockSystem.Singleton.LockOwner($"update-{vesselId}");
            }

            return !string.IsNullOrEmpty(owner) && WarpSystem.Singleton.PlayerIsInPastSubspace(owner);
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

        public static ISystem[] GetSingletons => new ISystem[]
        {
            VesselLockSystem.Singleton,
            VesselPositionSystem.Singleton,
            VesselFlightStateSystem.Singleton,
            VesselUpdateSystem.Singleton,
            VesselChangeSystem.Singleton,
            VesselProtoSystem.Singleton,
            VesselRemoveSystem.Singleton,
            VesselImmortalSystem.Singleton,
            VesselDockSystem.Singleton,
            VesselRangeSystem.Singleton
        };

        public static bool EnableAllSystems
        {
            set
            {
                if (value)
                {
                    VesselLockSystem.Singleton.Enabled = true;
                    VesselPositionSystem.Singleton.Enabled = true;
                    VesselFlightStateSystem.Singleton.Enabled = true;
                    VesselUpdateSystem.Singleton.Enabled = true;
                    VesselChangeSystem.Singleton.Enabled = true;
                    VesselProtoSystem.Singleton.Enabled = true;
                    VesselRemoveSystem.Singleton.Enabled = true;
                    VesselImmortalSystem.Singleton.Enabled = true;
                    VesselDockSystem.Singleton.Enabled = true;
                    VesselRangeSystem.Singleton.Enabled = true;
                }
                else
                {
                    VesselLockSystem.Singleton.Enabled = false;
                    VesselPositionSystem.Singleton.Enabled = false;
                    VesselFlightStateSystem.Singleton.Enabled = false;
                    VesselUpdateSystem.Singleton.Enabled = false;
                    VesselChangeSystem.Singleton.Enabled = false;
                    VesselProtoSystem.Singleton.Enabled = false;
                    VesselRemoveSystem.Singleton.Enabled = false;
                    VesselImmortalSystem.Singleton.Enabled = false;
                    VesselDockSystem.Singleton.Enabled = false;
                    VesselRangeSystem.Singleton.Enabled = false;
                }
            }
        }
    }
}
