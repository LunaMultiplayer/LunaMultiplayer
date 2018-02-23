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

        private static Guid _spectatingVesselId;
        public static Guid SpectatingVesselId
        {
            get => IsSpectating ? _spectatingVesselId : Guid.Empty;
            set => _spectatingVesselId = value;
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
                var loadedVesselIds = FlightGlobals.VesselsLoaded?.Select(v => v.id);

                if (loadedVesselIds != null)
                    return controlledVesselsIds.Intersect(loadedVesselIds).Any(v => v != FlightGlobals.ActiveVessel?.id);
            }

            return false;
        }

        /// <summary>
        /// Finds a part in a vessel without generating garbage. Returns null if not found
        /// </summary>
        public static Part FindPartInVessel(Vessel vessel, uint partFlightId)
        {
            if (vessel == null) return null;

            for (var i = 0; i < vessel.Parts.Count; i++)
            {
                if (vessel.Parts[i].flightID == partFlightId)
                    return vessel.Parts[i];
            }
            return null;
        }

        /// <summary>
        /// Finds a module in a part without generating garbage. Returns null if not found
        /// </summary>
        public static PartModule FindModuleInPart(Part part, string moduleName)
        {
            if (part == null) return null;

            for (var i = 0; i < part.Modules.Count; i++)
            {
                if (part.Modules[i].moduleName == moduleName)
                    return part.Modules[i];
            }

            return null;
        }

        /// <summary>
        /// Finds a module in a proto part module without generating garbage. Returns null if not found
        /// </summary>
        public static ProtoPartModuleSnapshot FindProtoPartModuleInProtoPart(ProtoPartSnapshot part, string moduleName)
        {
            if (part == null) return null;

            for (var i = 0; i < part.modules.Count; i++)
            {
                if (part.modules[i].moduleName == moduleName)
                    return part.modules[i];
            }

            return null;
        }

        /// <summary>
        /// Finds a proto part snapshot in a proto vessel without generating garbage. Returns null if not found
        /// </summary>
        public static ProtoPartSnapshot FindProtoPartInProtovessel(ProtoVessel protoVessel, uint partFlightId)
        {
            if (protoVessel == null) return null;

            for (var i = 0; i < protoVessel.protoPartSnapshots.Count; i++)
            {
                if (protoVessel.protoPartSnapshots[i].flightID == partFlightId)
                    return protoVessel.protoPartSnapshots[i];
            }
            return null;
        }

        /// <summary>
        /// Finds a resource in a part without generating garbage. Returns null if not found
        /// </summary>
        public static PartResource FindResourceInPart(Part part, string resourceName)
        {
            if (part == null) return null;

            for (var i = 0; i < part.Resources.Count; i++)
            {
                if (part.Resources[i].resourceName == resourceName)
                    return part.Resources[i];
            }
            return null;
        }

        /// <summary>
        /// Finds a proto part resource snapshot in a proto part snapshot without generating garbage. Returns null if not found
        /// </summary>
        public static ProtoPartResourceSnapshot FindResourceInProtoPart(ProtoPartSnapshot protoPart, string resourceName)
        {
            if (protoPart == null) return null;

            for (var i = 0; i < protoPart.resources.Count; i++)
            {
                if (protoPart.resources[i].resourceName == resourceName)
                    return protoPart.resources[i];
            }
            return null;
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
        /// Return all the vessels except the active one that we have the update lock and that are loaded
        /// </summary>
        public static IEnumerable<Vessel> GetUnloadedSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly

            return LockSystem.LockQuery.GetAllUnloadedUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(l => l.VesselId != FlightGlobals.ActiveVessel?.id)
                .Select(vi => FlightGlobals.FindVessel(vi.VesselId))
                .Where(v => v != null);
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

            return !String.IsNullOrEmpty(owner) && SystemsContainer.Get<WarpSystem>().PlayerIsInPastSubspace(owner);
        }

        /// <summary>
        /// Returns whether the given vessel is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(Vessel vessel)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel == null || vessel.mainBody?.flightGlobalsIndex != 1 || vessel.orbit?.referenceBody?.flightGlobalsIndex != 1)
                return false;

            //Use the protovessel values as the normal vessel values can be affected by the position system and the situation of the vessel
            return IsInSafetyBubble(vessel.protoVessel.latitude, vessel.protoVessel.longitude, vessel.protoVessel.altitude, 1);
        }

        /// <summary>
        /// Returns whether the given protovessel is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return true;

            if (protoVessel.orbitSnapShot != null)
                return IsInSafetyBubble(protoVessel.latitude, protoVessel.longitude, protoVessel.altitude, protoVessel.orbitSnapShot.ReferenceBodyIndex);

            return false;
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(double lat, double lon, double alt, int bodyIndex)
        {
            var body = FlightGlobals.Bodies[bodyIndex];
            if (body == null || body.name != "Kerbin")
                return false;

            return IsInSafetyBubble(FlightGlobals.Bodies[bodyIndex].GetWorldSurfacePosition(lat, lon, alt));
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(Vector3d position)
        {
            var landingPadDistance = Vector3d.Distance(position, LandingPadPosition);

            if (landingPadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance) return true;

            //We are far from the pad, let's see if happens the same in the runway...
            var runwayDistance = Vector3d.Distance(position, RunwayPosition);

            return runwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance;
        }

        /// <summary>
        /// Checks if the given config node from a protovessel has NaN orbits
        /// </summary>
        public static bool VesselHasNaNPosition(ConfigNode vesselNode)
        {
            if (vesselNode.GetValue("landed") == "True" || vesselNode.GetValue("splashed") == "True")
            {
                if (Double.TryParse(vesselNode.values.GetValue("lat"), out var latitude)
                    && (Double.IsNaN(latitude) || Double.IsInfinity(latitude)))
                    return true;

                if (Double.TryParse(vesselNode.values.GetValue("lon"), out var longitude)
                    && (Double.IsNaN(longitude) || Double.IsInfinity(longitude)))
                    return true;

                if (Double.TryParse(vesselNode.values.GetValue("alt"), out var altitude)
                    && (Double.IsNaN(altitude) || Double.IsInfinity(altitude)))
                    return true;
            }
            else
            {
                var orbitNode = vesselNode.GetNode("ORBIT");
                if (orbitNode != null)
                {
                    var allValuesAre0 = orbitNode.values.DistinctNames().Select(v => orbitNode.GetValue(v)).Take(7)
                        .All(v => v == "0");

                    return allValuesAre0 || orbitNode.values.DistinctNames().Select(v => orbitNode.GetValue(v))
                               .Any(val => Double.TryParse(val, out var value) && (Double.IsNaN(value) || Double.IsInfinity(value)));
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a protovessel from a ConfigNode
        /// </summary>
        public static ProtoVessel CreateSafeProtoVesselFromConfigNode(ConfigNode inputNode, Guid protoVesselId)
        {
            try
            {
                //Cannot create a protovessel if HighLogic.CurrentGame is null as we don't have a CrewRoster
                //and the protopartsnapshot constructor needs it
                if (HighLogic.CurrentGame == null)
                    return null;

                //Cannot reuse the Protovessel to save memory garbage as it does not have any clear method :(
                var pv = new ProtoVessel(inputNode, HighLogic.CurrentGame);
                foreach (var pps in pv.protoPartSnapshots)
                {
                    if (SystemsContainer.Get<ModSystem>().ModControl != ModControlMode.Disabled &&
                        !SystemsContainer.Get<ModSystem>().AllowedParts.Contains(pps.partName))
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
        /// Returns whether the given vessel is near KSC or not.  Measures distance from the landing pad / runway, so very small values may not cover all of KSC.
        /// </summary>
        public static bool IsNearKsc(ProtoVessel vessel, int distance)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel?.orbitSnapShot == null || vessel.orbitSnapShot?.ReferenceBodyIndex != 1)
                return false;

            var currentPos = FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex].GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude);

            var landingPadPosition = FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex].GetWorldSurfacePosition(LandingPadLatitude, LandingPadLongitude, KscAltitude);
            var landingPadDistance = Vector3d.Distance(currentPos, landingPadPosition);

            if (landingPadDistance < distance) return true;

            var runwayPosition = FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex].GetWorldSurfacePosition(RunwayLatitude, RunwayLongitude, KscAltitude);
            var runwayDistance = Vector3d.Distance(currentPos, runwayPosition);

            return runwayDistance < distance;
        }
    }
}
