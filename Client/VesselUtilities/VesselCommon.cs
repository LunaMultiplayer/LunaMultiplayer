using LunaClient.Systems;
using LunaClient.Systems.Chat;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.VesselUtilities
{
    /// <summary>
    /// Class to hold common logic regarding the Vessel systems
    /// </summary>
    public class VesselCommon
    {
        #region Fields and Properties for positions

        private const double LandingPadLatitude = -0.0971978130377757;
        private const double LandingPadLongitude = 285.44237039111;
        private const double RunwayLatitude = -0.0486001121594686;
        private const double RunwayLongitude = 285.275552559723;
        private const double KscAltitude = 60;

        private static CelestialBody _kerbin;

        private static CelestialBody Kerbin
        {
            get { return _kerbin ?? (_kerbin = FlightGlobals.Bodies.Find(b => b.bodyName == "Kerbin")); }
        }

        private static Vector3d LandingPadPosition =>
            Kerbin.GetWorldSurfacePosition(LandingPadLatitude, LandingPadLongitude, KscAltitude);

        private static Vector3d RunwayPosition =>
            Kerbin.GetWorldSurfacePosition(RunwayLatitude, RunwayLongitude, KscAltitude);

        #endregion

        public static bool UpdateIsForOwnVessel(Guid vesselId)
        {
            //Ignore updates to our own vessel if we aren't spectating
            return !IsSpectating && FlightGlobals.ActiveVessel?.id == vesselId;
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
        public static IEnumerable<Vessel> GetControlledVessels()
        {
            return GetControlledVesselIds()
                .Select(FlightGlobals.FindVessel);
        }

        /// <summary>
        /// Check if someone is spectating current vessel
        /// </summary>
        /// <returns></returns>
        public static bool IsSomeoneSpectatingUs => !IsSpectating && FlightGlobals.ActiveVessel != null &&
            LockSystem.LockQuery.SpectatorLockExists(FlightGlobals.ActiveVessel.id);

        /// <summary>
        /// Return the controlled vessel ids
        /// </summary>
        public static IEnumerable<Guid> GetControlledVesselIds()
        {
            return LockSystem.LockQuery.GetAllControlLocks()
                .Select(v => v.VesselId);
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
                var loadedVesselIds = FlightGlobals.VesselsLoaded.Select(v => v.id);

                return controlledVesselsIds.Intersect(loadedVesselIds).Any(v => v != FlightGlobals.ActiveVessel?.id);
            }

            return false;
        }

        /// <summary>
        /// Return all the vessels except the active one that we have the update lock and that are loaded
        /// </summary>
        public static IEnumerable<Vessel> GetSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly

            return LockSystem.LockQuery.GetAllUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(l => l.VesselId != FlightGlobals.ActiveVessel?.id)
                .Select(vi => FlightGlobals.VesselsLoaded.FirstOrDefault(v => v.id == vi.VesselId))
                .Where(v => v != null);
        }

        /// <summary>
        /// Return all the abandoned vessels (vessels that are not loaded and don't have update lock)
        /// </summary>
        public static IEnumerable<Vessel> GetAbandonedVessels()
        {
            return FlightGlobals.VesselsUnloaded.Where(v => !LockSystem.LockQuery.ControlLockExists(v.id));
        }

        /// <summary>
        /// Returns if given vessel is controlled and in a past subspace
        /// </summary>
        public static bool VesselIsControlledAndInPastSubspace(Guid vesselId)
        {
            var owner = "";
            if (LockSystem.LockQuery.ControlLockExists(vesselId))
            {
                owner = LockSystem.LockQuery.GetControlLockOwner(vesselId);
            }
            else if (LockSystem.LockQuery.UpdateLockExists(vesselId))
            {
                owner = LockSystem.LockQuery.GetUpdateLockOwner(vesselId);
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
            if (vessel == null || vessel.mainBody?.name != "Kerbin")
                return false;

            var landingPadDistance = Vector3d.Distance(vessel.GetWorldPos3D(), LandingPadPosition);

            if (landingPadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance) return true;

            //We are far from the pad, let's see if happens the same in the runway...
            var runwayDistance = Vector3d.Distance(vessel.GetWorldPos3D(), RunwayPosition);

            return runwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance;
        }

        /// <summary>
        /// Creates a protovessel from a ConfigNode
        /// </summary>
        public static ProtoVessel CreateSafeProtoVesselFromConfigNode(ConfigNode inputNode, Guid protoVesselId)
        {
            try
            {
                //TODO: Check if this can be improved as it probably creates a lot of garbage in memory
                var pv = new ProtoVessel(inputNode, HighLogic.CurrentGame);
                var cn = new ConfigNode();
                pv.Save(cn);

                foreach (var pps in pv.protoPartSnapshots)
                {
                    if (SystemsContainer.Get<ModSystem>().ModControl != ModControlMode.Disabled &&
                        !SystemsContainer.Get<ModSystem>().AllowedParts.Contains(pps.partName.ToLower()))
                    {
                        var msg = $"[LMP]: WARNING: Protovessel {protoVesselId} ({pv.vesselName}) contains the banned " +
                                  $"part '{pps.partName}'!. Skipping load.";

                        LunaLog.LogWarning(msg);
                        SystemsContainer.Get<ChatSystem>().PmMessageServer(msg);

                        return null;
                    }
                    if (pps.partInfo == null)
                    {
                        var msg = $"[LMP]: WARNING: Protovessel {protoVesselId} ({pv.vesselName}) contains the missing " +
                                  $"part '{pps.partName}'!. Skipping load.";

                        LunaLog.LogWarning(msg);
                        SystemsContainer.Get<ChatSystem>().PmMessageServer(msg);

                        ScreenMessages.PostScreenMessage($"Cannot load '{pv.vesselName}' - you are missing {pps.partName}", 10f,
                            ScreenMessageStyle.UPPER_CENTER);

                        return null;
                    }

                    var missingeResource = pps.resources
                        .FirstOrDefault(r => !PartResourceLibrary.Instance.resourceDefinitions.Contains(r.resourceName));

                    if (missingeResource != null)
                    {
                        var msg = $"[LMP]: WARNING: Protovessel {protoVesselId} ({pv.vesselName}) " +
                                  $"contains the missing resource '{missingeResource.resourceName}'!. Skipping load.";

                        LunaLog.LogWarning(msg);
                        SystemsContainer.Get<ChatSystem>().PmMessageServer(msg);

                        ScreenMessages.PostScreenMessage($"Cannot load '{pv.vesselName}' - you are missing the resource " +
                                                         $"{missingeResource.resourceName}", 10f, ScreenMessageStyle.UPPER_CENTER);
                        return null;
                    }
                }
                return pv;
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Damaged vessel {protoVesselId}, exception: {e}");
                return null;
            }
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
        
        /// <summary>
        /// This method determines if the vessel must be reloaded. 
        /// Bear in mind that when you reload a vessel there's a small flickering
        /// </summary>
        public static bool ProtoVesselNeedsToBeReloaded(Vessel existing, ProtoVessel newProtoVessel)
        {
            if (existing == null || newProtoVessel == null) return true;

            if (!existing.loaded)
            {
                //Wenever we receive a proto of a unloaded vessel just reload 
                //it at will as we cannot compare it against anything as it's not even in range
                //and we don't know what panels can be extended, etc
                    return true;
            }

            if (existing.Parts.Count != newProtoVessel.protoPartSnapshots.Count)
                return true;

            if (existing.situation != newProtoVessel.situation)
                return true;

            return false;
        }
    }
}
