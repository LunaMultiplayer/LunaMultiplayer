using LmpClient.Network;
using LmpClient.Systems.Chat;
using LmpClient.Systems.Lock;
using LmpClient.Systems.Mod;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.VesselFairingsSys;
using LmpClient.Systems.VesselFlightStateSys;
using LmpClient.Systems.VesselPartSyncCallSys;
using LmpClient.Systems.VesselPartSyncFieldSys;
using LmpClient.Systems.VesselPartSyncUiFieldSys;
using LmpClient.Systems.VesselPositionSys;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.VesselResourceSys;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// Class to hold common logic regarding the Vessel systems
    /// </summary>
    public class VesselCommon
    {
        public static float PositionAndFlightStateMessageOffsetSec(float targetPingMs) => (float)TimeSpan.FromMilliseconds(PositionAndFlightStateMessageOffsetMs(targetPingMs)).TotalSeconds;
        public static int PositionAndFlightStateMessageOffsetMs(float targetPingMs) => (int)(NetworkStatistics.PingMs + targetPingMs * 1.1f);

        public static bool UpdateIsForOwnVessel(Guid vesselId)
        {
            //Ignore updates to our own vessel if we aren't spectating
            return !IsSpectating && FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == vesselId;
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
        /// Removes the specified vessel from the vessel systems
        /// </summary>
        public static void RemoveVesselFromSystems(Guid vesselId)
        {
            VesselPositionSystem.Singleton.RemoveVessel(vesselId);
            VesselFlightStateSystem.Singleton.RemoveVessel(vesselId);
            VesselResourceSystem.Singleton.RemoveVessel(vesselId);
            VesselProtoSystem.Singleton.RemoveVessel(vesselId);
            VesselPartSyncFieldSystem.Singleton.RemoveVessel(vesselId);
            VesselPartSyncUiFieldSystem.Singleton.RemoveVessel(vesselId);
            VesselPartSyncCallSystem.Singleton.RemoveVessel(vesselId);
            VesselFairingsSystem.Singleton.RemoveVessel(vesselId);
        }

        /// <summary>
        /// Check if there are other player controlled vessels nearby
        /// </summary>
        /// <returns></returns>
        public static bool PlayerVesselsNearby()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < FlightGlobals.VesselsLoaded.Count; i++)
                {
                    if (FlightGlobals.VesselsLoaded[i] != FlightGlobals.ActiveVessel)
                        return true;
                }

                return false;

                //TODO: I simplified this method as it generates a lot of garbage since it's called on every frame
                ////If there is someone spectating us then return true and update it fast;
                //if (IsSomeoneSpectatingUs)
                //{
                //    return true;
                //}

                //var controlledVesselsIds = GetControlledVesselIds();
                //var loadedVesselIds = FlightGlobals.VesselsLoaded?.Where(v => v != null).Select(v => v.id);

                //if (loadedVesselIds != null)
                //    return controlledVesselsIds.Intersect(loadedVesselIds).Any(v => v != FlightGlobals.ActiveVessel?.id);
            }

            return false;
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
                    LunaScreenMsg.PostScreenMessage($"Cannot load '{pv.vesselName}' - missing part: {pps.partName}", 10f, ScreenMessageStyle.UPPER_CENTER);

                    return true;
                }

                var missingeResource = pps.resources.FirstOrDefault(r => !PartResourceLibrary.Instance.resourceDefinitions.Contains(r.resourceName));
                if (missingeResource != null)
                {
                    var msg = $"Protovessel {pv.vesselID} ({pv.vesselName}) contains the MISSING RESOURCE '{missingeResource.resourceName}'. Skipping load.";
                    LunaLog.LogWarning(msg);
                    ChatSystem.Singleton.PmMessageServer(msg);

                    LunaScreenMsg.PostScreenMessage($"Cannot load '{pv.vesselName}' - missing resource: {missingeResource.resourceName}", 10f, ScreenMessageStyle.UPPER_CENTER);
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
            //if (FlightGlobals.FindUnloadedPart(partPersistentId, out var existingProtoPart))
            //{
            //    return existingProtoPart;
            //}

            //if (FlightGlobals.FindLoadedPart(partPersistentId, out var existingPart))
            //{
            //    return existingPart.protoPartSnapshot;
            //}

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
                .Where(l => FlightGlobals.ActiveVessel && l.VesselId != FlightGlobals.ActiveVessel.id)
                .Select(vi => FlightGlobals.VesselsLoaded.FirstOrDefault(v => v != null && v.id == vi.VesselId))
                .Where(v => v != null && v.id != Guid.Empty);
        }

        /// <summary>
        /// Return all the that we have the unloaded update lock ONLY.
        /// </summary>
        public static IEnumerable<Vessel> GetUnloadedSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly
            return LockSystem.LockQuery.GetAllUnloadedUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(l => FlightGlobals.ActiveVessel && l.VesselId != FlightGlobals.ActiveVessel.id && !LockSystem.LockQuery.UpdateLockExists(l.VesselId))
                .Select(vi => FlightGlobals.VesselsUnloaded.FirstOrDefault(v => v != null && v.id == vi.VesselId))
                .Where(v => v != null && v.id != Guid.Empty);
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
