using System;
using Expansions.Serenity.DeployedScience.Runtime;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using UnityEngine;

namespace LmpClient.Systems.Scenario
{
    public static class DeployedScienceRelinker
    {
        //Stock caches the cluster in a private field whose only writer is the lazy getter; a
        //module that cached the previous join's orphaned cluster never re-resolves after the
        //deferred OnLoad replaces the dictionary. Null the cache so the stock lazy path restores.
        private static readonly System.Reflection.FieldInfo ClusterCacheField =
            typeof(ModuleGroundSciencePart).GetField("scienceClusterData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.MethodInfo UpdateModuleUiMethod =
            typeof(ModuleGroundSciencePart).GetMethod("UpdateModuleUI",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        //Mirror stock's protected tracking fields so actual output is recomputed, not forced to potential.
        private static readonly System.Reflection.FieldInfo SecondaryTransformField =
            typeof(ModuleGroundSciencePart).GetField("secondaryTransform",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo TrackingBodyField =
            typeof(ModuleGroundSciencePart).GetField("trackingBody",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static void RelinkAfterDeferredOnLoad(DeployedScience deployedScience)
        {
            if (deployedScience?.DeployedScienceClusters == null)
                return;

            foreach (var cluster in deployedScience.DeployedScienceClusters.Values)
            {
                RestoreClusterPowerInputs(cluster);
                cluster.UpdatePowerState();
                InvalidateStaleClusterCaches(cluster);

                var control = FindControlModule(cluster);
                if (control == null)
                {
                    // Packed controller is normal (station out of physics range); modules re-bind on load.
                    if (!FlightGlobals.FindUnloadedPart(cluster.ControlModulePartId, out _))
                        LunaLog.LogError($"[LMP]: DeployedScience relink: cluster {cluster.ControlModulePartId} - control part not found in FlightGlobals (neither loaded nor packed). Live-module events skipped.");
                }
                else
                {
                    GameEvents.onGroundScienceClusterUpdated.Fire(control, cluster);
                    GameEvents.onGroundScienceClusterPowerStateChanged.Fire(cluster);
                    InvokeStockControllerRefresh(control);
                    MonoUtilities.RefreshContextWindows(control.part);
                }

                SyncSolarPanelRuntimeValues(cluster);
                RelinkExperiments(cluster);
            }
        }

        private static void RestoreClusterPowerInputs(DeployedScienceCluster cluster)
        {
            var parts = cluster.DeployedScienceParts;
            if (parts == null)
                return;

            foreach (var part in parts)
            {
                if (part == null || !part.IsSolarPanel)
                    continue;

                var livePart = part.PartIsLoaded();
                ProtoPartSnapshot snapshot = null;
                if (livePart != null)
                {
                    var snapshots = livePart.vessel?.protoVessel?.protoPartSnapshots;
                    if (snapshots != null)
                        for (var i = 0; i < snapshots.Count; i++)
                            if (snapshots[i] != null && snapshots[i].flightID == livePart.flightID)
                                snapshot = snapshots[i];
                }
                else
                {
                    FlightGlobals.FindUnloadedPart(part.PartId, out snapshot);
                }

                var moduleValues = FindProtoModuleValues(snapshot, "ModuleGroundSciencePart");
                if (moduleValues == null)
                    continue;

                var protoEnabled = ParseBool(moduleValues.GetValue("isEnabled"));
                var protoDeployed = ParseBool(moduleValues.GetValue("deployedOnGround"));
                var protoDeploying = ParseBool(moduleValues.GetValue("beingDeployed"));
                var protoProduced = ParseInt(moduleValues.GetValue("powerUnitsProduced"));

                if (!part.Enabled && protoEnabled == true && protoDeployed == true && protoDeploying != true)
                {
                    part.Enabled = true;
                }

                var liveModule = livePart?.FindModuleImplementing<ModuleGroundSciencePart>();
                if (liveModule != null && protoProduced.HasValue && liveModule.PowerUnitsProduced != protoProduced.Value)
                {
                    liveModule.PowerUnitsProduced = protoProduced.Value;
                }
            }
        }

        private static void SyncSolarPanelRuntimeValues(DeployedScienceCluster cluster)
        {
            if (cluster.DeployedScienceParts == null)
                return;

            foreach (var part in cluster.DeployedScienceParts)
            {
                if (part == null || !part.IsSolarPanel)
                    continue;

                var livePart = part.PartIsLoaded();
                var liveModule = livePart?.FindModuleImplementing<ModuleGroundSciencePart>();
                if (liveModule == null)
                    continue;

                Transform secondary = null;
                CelestialBody trackingBody = null;
                if (SecondaryTransformField != null)
                    secondary = SecondaryTransformField.GetValue(liveModule) as Transform;
                if (TrackingBodyField != null)
                    trackingBody = TrackingBodyField.GetValue(liveModule) as CelestialBody;

                bool hasLineOfSight = false;
                if (secondary != null && trackingBody != null && livePart.partTransform != null)
                {
                    var dir = (trackingBody.position - livePart.partTransform.position).normalized;
                    string blocker = null;
                    try { hasLineOfSight = liveModule.CalculateTrackingLOS(dir, ref blocker); }
                    catch (Exception e)
                    {
                        LunaLog.LogWarning($"[LMP]: DeployedScience relink: CalculateTrackingLOS failed for part {part.PartId}: {e.Message}");
                        continue;
                    }
                }

                var target = SolarPanelActualOutputPolicy.ComputeTarget(
                    moduleBehaviourEnabled: liveModule.enabled,
                    hasSecondaryTransform: secondary != null,
                    hasTrackingBody: trackingBody != null,
                    timeWarpRateIsOne: TimeWarp.CurrentRate == 1f,
                    hasLineOfSight: hasLineOfSight,
                    deployedOnGround: liveModule.DeployedOnGround,
                    isEnabled: liveModule.isEnabled,
                    potentialOutput: liveModule.PowerUnitsProduced);

                //null target = stock freezes the last value (timewarp / no tracking body) — do not write.
                if (target.HasValue && liveModule.ActualPowerUnitsProduced != target.Value)
                {
                    liveModule.ActualPowerUnitsProduced = target.Value; //setter fires onGroundSciencePartChanged
                    MonoUtilities.RefreshContextWindows(livePart);
                }
            }
        }

        private static bool? ParseBool(string value)
        {
            if (bool.TryParse(value, out var result)) return result;
            return null;
        }

        private static int? ParseInt(string value)
        {
            if (int.TryParse(value, out var result)) return result;
            return null;
        }

        private static void InvalidateStaleClusterCaches(DeployedScienceCluster cluster)
        {
            if (ClusterCacheField == null)
            {
                LunaLog.LogWarning("[LMP]: DeployedScience relink: ModuleGroundSciencePart.scienceClusterData field not found - stale-cache invalidation skipped");
                return;
            }

            try
            {
                var vessels = FlightGlobals.Vessels;
                if (vessels == null) return;

                for (var i = 0; i < vessels.Count; i++)
                {
                    var vessel = vessels[i];
                    if (vessel == null || !vessel.loaded || vessel.parts == null) continue;

                    for (var p = 0; p < vessel.parts.Count; p++)
                    {
                        var part = vessel.parts[p];
                        var module = part?.FindModuleImplementing<ModuleGroundSciencePart>();
                        if (module == null || module.ControlUnitId != cluster.ControlModulePartId) continue;

                        if (ClusterCacheField.GetValue(module) != null)
                        {
                            ClusterCacheField.SetValue(module, null);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: DeployedScience relink: cluster cache invalidation failed: {e.Message}");
            }
        }

        public static void RefreshControllerUi(ModuleGroundExpControl control)
        {
            if (control == null) return;

            InvokeStockControllerRefresh(control);
            if (control.part != null)
                MonoUtilities.RefreshContextWindows(control.part);
        }

        private static void InvokeStockControllerRefresh(ModuleGroundExpControl control)
        {
            if (UpdateModuleUiMethod == null)
            {
                LunaLog.LogWarning("[LMP]: DeployedScience relink: ModuleGroundSciencePart.UpdateModuleUI not found - controller PAW refresh left to stock PAW-open path");
                return;
            }

            try
            {
                UpdateModuleUiMethod.Invoke(control, null);
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: DeployedScience relink: stock controller UpdateModuleUI invoke failed: {e.Message}");
            }
        }

        private static ModuleGroundExpControl FindControlModule(DeployedScienceCluster cluster)
        {
            uint controlPartId = cluster.ControlModulePartId;
            return FlightGlobals.FindLoadedPart(controlPartId, out var part) && part != null
                ? part.FindModuleImplementing<ModuleGroundExpControl>()
                : null;
        }

        private static void RelinkExperiments(DeployedScienceCluster cluster)
        {
            foreach (var part in cluster.DeployedScienceParts)
            {
                var experiment = part.Experiment; // null for solar panels / antennas
                if (experiment == null)
                    continue;

                var module = experiment.GroundExperimentModule; // null when the vessel is unloaded
                if (module == null)
                    continue;

                module.ScienceModifierRate = experiment.ScienceModifierRate;
                module.ScienceLimit = experiment.ScienceLimit;
                module.ScienceValue = experiment.ScienceValue;
                MonoUtilities.RefreshContextWindows(module.part);
            }
        }

        private static global::ConfigNode FindProtoModuleValues(global::ProtoPartSnapshot snapshot, string moduleName)
        {
            try
            {
                var modules = snapshot?.modules;
                if (modules == null)
                    return null;

                for (var i = 0; i < modules.Count; i++)
                {
                    var moduleSnapshot = modules[i];
                    if (moduleSnapshot != null && moduleSnapshot.moduleName == moduleName)
                        return moduleSnapshot.moduleValues;
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
