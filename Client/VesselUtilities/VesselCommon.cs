using LunaClient.Systems.Chat;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.Warp;
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

        private const double KscLaunchpadLatitude = -0.0972073656774383;
        private const double KscLaunchpadLongitude = -74.5576690686149;
        private const double KscLaunchpadAltitude = 74.5676483437419;

        private const double KscRunwayLatitude = -0.0485997166968939;
        private const double KscRunwayLongitude = -74.7244722554176;
        private const double KscRunwayAltitude = 71.2173345269402;

        private const double DesertLaunchpadLatitude = -6.56038147097707;
        private const double DesertLaunchpadLongitude = -143.950039339818;
        private const double DesertLaunchpadAltitude = 826.345226691803;

        private const double DesertRunwayLatitude = -6.59970939244927;
        private const double DesertRunwayLongitude = -144.040462582416;
        private const double DesertRunwayAltitude = 823.02435584378;

        private const double WoomerangLaunchpadLatitude = 45.2896282963322;
        private const double WoomerangLaunchpadLongitude = 136.109992206036;
        private const double WoomerangLaunchpadAltitude = 741.653213000041;

        private const double IslandRunwayLatitude = -1.52927593838872;
        private const double IslandRunwayLongitude = -71.8853164314488;
        private const double IslandRunwayAltitude = 135.12223753822;

        private static CelestialBody _homeBody;
        private static CelestialBody HomeBody => _homeBody ?? (_homeBody = FlightGlobals.Bodies.Find(b => b.isHomeWorld));

        private static Vector3d KscLaunchpadPosition => HomeBody.GetWorldSurfacePosition(KscLaunchpadLatitude, KscLaunchpadLongitude, KscLaunchpadAltitude);
        private static Vector3d KscRunwayPosition => HomeBody.GetWorldSurfacePosition(KscRunwayLatitude, KscRunwayLongitude, KscRunwayAltitude);
        private static Vector3d DesertLaunchpadPosition => HomeBody.GetWorldSurfacePosition(DesertLaunchpadLatitude, DesertLaunchpadLongitude, DesertLaunchpadAltitude);
        private static Vector3d DesertRunwayPosition => HomeBody.GetWorldSurfacePosition(DesertRunwayLatitude, DesertRunwayLongitude, DesertRunwayAltitude);
        private static Vector3d WoomerangLaunchpadPosition => HomeBody.GetWorldSurfacePosition(WoomerangLaunchpadLatitude, WoomerangLaunchpadLongitude, WoomerangLaunchpadAltitude);
        private static Vector3d IslandRunwayPosition => HomeBody.GetWorldSurfacePosition(IslandRunwayLatitude, IslandRunwayLongitude, IslandRunwayAltitude);

        #endregion

        public static bool UpdateIsForOwnVessel(Guid vesselId)
        {
            //Ignore updates to our own vessel if we aren't spectating
            return !IsSpectating && FlightGlobals.ActiveVessel?.id == vesselId;
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
        /// Finds a part in a vessel without generating garbage. Returns null if not found
        /// </summary>
        public static Part FindPartInVessel(Vessel vessel, string partName)
        {
            if (vessel == null) return null;

            for (var i = 0; i < vessel.Parts.Count; i++)
            {
                if (vessel.Parts[i].partName == partName)
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
        /// Check if we should apply a message to the given vesselId
        /// </summary>
        public static bool DoVesselChecks(Guid vesselId)
        {
            //Ignore updates if vessel is in kill list
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(vesselId))
                return false;

            //Ignore vessel updates for our own controlled vessel
            if (LockSystem.LockQuery.ControlLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return false;

            //Ignore vessel updates for our own updated vessels
            if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return false;

            //Ignore vessel updates for our own updated vessels
            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the protovessel has resources,parts that you don't have or that they are banned
        /// </summary>
        public static bool ProtoVesselHasInvalidParts(ProtoVessel pv)
        {
            foreach (var pps in pv.protoPartSnapshots)
            {
                if (ModSystem.Singleton.ModControl && !ModSystem.Singleton.AllowedParts.Contains(pps.partName))
                {
                    var msg = $"Protovessel {pv.vesselID} ({pv.vesselName}) contains the BANNED PART '{pps.partName}'. Skipping load.";
                    LunaLog.LogWarning(msg);
                    ChatSystem.Singleton.PmMessageServer(msg);

                    return true;
                }

                if (pps.partInfo == null)
                {
                    LunaLog.LogWarning($"Protovessel {pv.vesselID} ({pv.vesselName}) contains the MISSING PART '{pps.partName}'. Skipping load.");
                    LunaScreenMsg.PostScreenMessage($"Cannot load '{pv.vesselName}' - missing {pps.partName}", 10f, ScreenMessageStyle.UPPER_CENTER);

                    return true;
                }

                var missingeResource = pps.resources.FirstOrDefault(r => !PartResourceLibrary.Instance.resourceDefinitions.Contains(r.resourceName));
                if (missingeResource != null)
                {
                    var msg = $"Protovessel {pv.vesselID} ({pv.vesselName}) contains the MISSING RESOURCE '{missingeResource.resourceName}'. Skipping load.";
                    LunaLog.LogWarning(msg);
                    ChatSystem.Singleton.PmMessageServer(msg);

                    LunaScreenMsg.PostScreenMessage($"Cannot load '{pv.vesselName}' - missing resource {missingeResource.resourceName}", 10f, ScreenMessageStyle.UPPER_CENTER);
                    return true;
                }
            }

            return false;
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
        /// Finds a proto part snapshot in a proto vessel without generating garbage. Returns null if not found
        /// </summary>
        public static ProtoPartSnapshot FindProtoPartInProtovessel(ProtoVessel protoVessel, string partName)
        {
            if (protoVessel == null) return null;

            for (var i = 0; i < protoVessel.protoPartSnapshots.Count; i++)
            {
                if (protoVessel.protoPartSnapshots[i].partName == partName)
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
                .Where(v => v != null && v.id != Guid.Empty);
        }

        /// <summary>
        /// Return all the that we have the unloaded update lock ONLY.
        /// </summary>
        public static IEnumerable<Vessel> GetUnloadedSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly
            return LockSystem.LockQuery.GetAllUnloadedUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(l => l.VesselId != FlightGlobals.ActiveVessel?.id && !LockSystem.LockQuery.UpdateLockExists(l.VesselId))
                .Select(vi => FlightGlobals.VesselsUnloaded.FirstOrDefault(v => v.id == vi.VesselId))
                .Where(v => v != null && v.id != Guid.Empty);
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

            return !string.IsNullOrEmpty(owner) && WarpSystem.Singleton.PlayerIsInPastSubspace(owner);
        }

        /// <summary>
        /// Returns whether the given vessel is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(Vessel vessel, bool useLatLonAltFromProto = true)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel == null || !vessel.mainBody.isHomeWorld)
                return false;

            if (vessel.situation >= Vessel.Situations.FLYING)
                return false;

            if (useLatLonAltFromProto)
                //Use the protovessel values as the normal vessel values can be affected by the position system and the situation of the vessel
                return IsInSafetyBubble(vessel.protoVessel.latitude, vessel.protoVessel.longitude, vessel.protoVessel.altitude, vessel.mainBody);

            return IsInSafetyBubble(vessel.latitude, vessel.longitude, vessel.altitude, vessel.mainBody);
        }

        /// <summary>
        /// Returns whether the given protovessel is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(ProtoVessel protoVessel)
        {
            if (protoVessel == null)
                return true;
            
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
            if (body == null || !body.isHomeWorld)
                return false;

            return IsInSafetyBubble(FlightGlobals.Bodies[bodyIndex].GetWorldSurfacePosition(lat, lon, alt));
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(double lat, double lon, double alt, CelestialBody body)
        {
            if (body == null || !body.isHomeWorld)
                return false;

            return IsInSafetyBubble(body.GetWorldSurfacePosition(lat, lon, alt));
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(Vector3d position)
        {
            var kscLaunchpadDistance = Vector3d.Distance(position, KscLaunchpadPosition);
            var kscRunwayDistance = Vector3d.Distance(position, KscRunwayPosition);
            var desertLaunchpadDistance = Vector3d.Distance(position, DesertLaunchpadPosition);
            var desertRunwayDistance = Vector3d.Distance(position, DesertRunwayPosition);
            var woomerangLaunchpadDistance = Vector3d.Distance(position, WoomerangLaunchpadPosition);
            var islandRunwayDistance = Vector3d.Distance(position, IslandRunwayPosition);
            
            return kscLaunchpadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   kscRunwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   desertLaunchpadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   desertRunwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   woomerangLaunchpadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   islandRunwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance;
        }

        /// <summary>
        /// Checks if the given config node from a protovessel has NaN orbits
        /// </summary>
        public static bool VesselHasNaNPosition(ConfigNode vesselNode)
        {
            if (vesselNode.GetValue("landed") == "True" || vesselNode.GetValue("splashed") == "True")
            {
                if (double.TryParse(vesselNode.values.GetValue("lat"), out var latitude)
                    && (double.IsNaN(latitude) || double.IsInfinity(latitude)))
                    return true;

                if (double.TryParse(vesselNode.values.GetValue("lon"), out var longitude)
                    && (double.IsNaN(longitude) || double.IsInfinity(longitude)))
                    return true;

                if (double.TryParse(vesselNode.values.GetValue("alt"), out var altitude)
                    && (double.IsNaN(altitude) || double.IsInfinity(altitude)))
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
                               .Any(val => double.TryParse(val, out var value) && (double.IsNaN(value) || double.IsInfinity(value)));
                }
            }

            return false;
        }
    }
}
