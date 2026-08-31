using System;
using System.Collections.Generic;
using Expansions.Serenity.DeployedScience.Runtime;
using LmpClient.Utilities;

namespace LmpClient.Systems.Scenario
{
    public static class DeployedScienceLiveRegistration
    {
        private static readonly System.Reflection.FieldInfo BeingDeployedField =
            typeof(ModuleGroundPart).GetField("beingDeployed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static void EvaluatePendingRegistrations()
        {
            //Only after the authoritative scenario applied — earlier clusters are the previous join's.
            if (!DeployedScienceSyncGate.DeployedScienceReady)
                return;

            var deployedScience = DeployedScience.Instance;
            if (deployedScience?.DeployedScienceClusters == null || deployedScience.DeployedScienceClusters.Count == 0)
                return;

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
                        if (module == null) continue;

                        EvaluateModule(deployedScience, part, module);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: GroundScience live registration sweep failed: {e.Message}");
            }
        }

        private static void EvaluateModule(DeployedScience deployedScience, global::Part part, ModuleGroundSciencePart module)
        {
            deployedScience.DeployedScienceClusters.TryGetValue(module.ControlUnitId, out var cluster);

            var clusterFound = cluster != null;
            var clusterContainsPart = false;
            var isExperiment = false;
            var clusterHasSameExperiment = false;

            if (clusterFound && cluster.DeployedScienceParts != null)
            {
                clusterContainsPart = cluster.DeployedScienceParts.Get(part.persistentId) != null;

                if (module is ModuleGroundExperiment experimentModule)
                {
                    isExperiment = true;
                    clusterHasSameExperiment = ClusterHasExperiment(cluster, experimentModule.experimentId);
                }
            }

            if (!DeployedScienceLiveRegistrationPolicy.ShouldRegister(
                    deployedOnGround: module.DeployedOnGround,
                    beingDeployed: IsBeingDeployed(module),
                    controlUnitIdAssigned: module.ControlUnitId != 0,
                    clusterFound: clusterFound,
                    clusterContainsPart: clusterContainsPart,
                    isExperiment: isExperiment,
                    clusterHasSameExperiment: clusterHasSameExperiment))
                return;

            var control = FindLiveController(module.ControlUnitId);
            if (control == null)
            {
                // Controller out of physics range is normal; retried every sweep.
                return;
            }

            LunaLog.Log($"[LMP]: GroundScience live registration: part {part.persistentId} (controller {module.ControlUnitId}) missing from live cluster {cluster.ControlModulePartId} - replaying stock onGroundScienceControllerChanged");

            var parts = new List<ModuleGroundSciencePart> { module };
            GameEvents.onGroundScienceControllerChanged.Fire(control, false, parts);

            DeployedScienceRelinker.RefreshControllerUi(control);
        }

        private static bool _beingDeployedFieldMissingLogged;

        private static bool IsBeingDeployed(ModuleGroundSciencePart module)
        {
            if (module == null) return false;
            if (BeingDeployedField == null)
            {
                if (!_beingDeployedFieldMissingLogged)
                {
                    LunaLog.LogWarning("[LMP]: beingDeployed reflection field missing; treating all modules as deploying (fail-closed)");
                    _beingDeployedFieldMissingLogged = true;
                }
                return true;
            }
            return BeingDeployedField.GetValue(module) is bool beingDeployed && beingDeployed;
        }

        private static bool ClusterHasExperiment(DeployedScienceCluster cluster, string experimentId)
        {
            var parts = cluster.DeployedScienceParts;
            for (var i = 0; i < parts.Count; i++)
            {
                var experiment = parts[i] != null ? parts[i].Experiment : null;
                if (experiment != null && experiment.ExperimentId == experimentId)
                    return true;
            }
            return false;
        }

        private static ModuleGroundExpControl FindLiveController(uint controlUnitId)
        {
            return FlightGlobals.FindLoadedPart(controlUnitId, out var part) && part != null
                ? part.FindModuleImplementing<ModuleGroundExpControl>()
                : null;
        }
    }
}
