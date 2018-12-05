using LmpClient.Network;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.VesselCoupleSys;
using LmpClient.Systems.VesselDecoupleSys;
using LmpClient.Systems.VesselFairingsSys;
using LmpClient.Systems.VesselFlightStateSys;
using LmpClient.Systems.VesselPartSyncCallSys;
using LmpClient.Systems.VesselPartSyncFieldSys;
using LmpClient.Systems.VesselPartSyncUiFieldSys;
using LmpClient.Systems.VesselPositionSys;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.VesselResourceSys;
using LmpClient.Systems.VesselUndockSys;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// Class to hold common logic regarding the Vessel systems
    /// </summary>
    public class VesselCommon
    {
        public static float PositionAndFlightStateMessageOffsetSec(float targetPingMs) => (float)TimeSpan.FromMilliseconds(PositionAndFlightStateMessageOffsetMs(targetPingMs)).TotalSeconds;
        public static int PositionAndFlightStateMessageOffsetMs(float targetPingMs) => (int)Mathf.Clamp(NetworkStatistics.PingMs + targetPingMs, 250, 1000);

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
            VesselCoupleSystem.Singleton.RemoveVessel(vesselId);
            VesselDecoupleSystem.Singleton.RemoveVessel(vesselId);
            VesselUndockSystem.Singleton.RemoveVessel(vesselId);
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
                .Select(l => FlightGlobals.VesselsLoaded.FirstOrDefault(v => v && v.id == l.VesselId))
                .Where(v => v && (FlightGlobals.ActiveVessel == null || v != FlightGlobals.ActiveVessel));
        }

        /// <summary>
        /// Return all the that we have the unloaded update lock ONLY.
        /// </summary>
        public static IEnumerable<Vessel> GetUnloadedSecondaryVessels()
        {
            //We don't need to check if vessel is in safety bubble as the update locks are updated accordingly
            return LockSystem.LockQuery.GetAllUnloadedUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Select(l => FlightGlobals.VesselsUnloaded.FirstOrDefault(v => v && v.id == l.VesselId))
                .Where(v => v && (FlightGlobals.ActiveVessel == null || v != FlightGlobals.ActiveVessel));
        }
    }
}
