using System;
using System.Collections.Generic;

namespace LmpClient.Systems.Scenario
{
    public static class DeployedScienceSyncGate
    {
        public const string ScenarioModuleName = "DeployedScience";

        public static bool DeployedScienceReady { get; set; }

        public static bool ShouldSend(string scenarioModuleName)
        {
            if (!string.Equals(scenarioModuleName, ScenarioModuleName, StringComparison.Ordinal))
                return true;
            return DeployedScienceReady;
        }

        public static bool ShouldMarkFreshModuleAuthoritative(bool receivedScenarioEntry, bool vesselSyncComplete, bool alreadyReady)
        {
            return !receivedScenarioEntry && vesselSyncComplete && !alreadyReady;
        }

        public sealed class ClusterMembership
        {
            public readonly HashSet<uint> PartIds = new HashSet<uint>();
            public readonly HashSet<string> ExperimentIds = new HashSet<string>(StringComparer.Ordinal);
        }

        public static bool IsMembershipApplied(IReadOnlyDictionary<uint, ClusterMembership> declared, IReadOnlyDictionary<uint, ClusterMembership> applied)
        {
            if (declared.Count != applied.Count)
                return false;

            foreach (var pair in declared)
            {
                if (!applied.TryGetValue(pair.Key, out var appliedCluster))
                    return false;

                if (!pair.Value.PartIds.SetEquals(appliedCluster.PartIds))
                    return false;

                if (!pair.Value.ExperimentIds.SetEquals(appliedCluster.ExperimentIds))
                    return false;
            }

            return true;
        }
    }

    public class VesselSyncCompletionTracker
    {
        private readonly double _graceSeconds;
        private bool _isComplete;
        private bool _wasReady;
        private double? _readySince;
        private double? _lastActivity;

        public VesselSyncCompletionTracker(double graceSeconds)
        {
            _graceSeconds = graceSeconds;
        }

        public bool IsComplete => _isComplete;

        public void Update(double now, bool protoSystemReady, bool anyProtoPending, bool vesselLoadedThisTick)
        {
            if (_isComplete)
                return; // latched

            if (!protoSystemReady)
            {
                _wasReady = false;
                _readySince = null;
                _lastActivity = null;
                return;
            }

            bool hasActivity = anyProtoPending || vesselLoadedThisTick;
            if (hasActivity)
            {
                //Completion is measured from the last observed activity.
                _lastActivity = now;
                _readySince = null;
                return;
            }

            if (_lastActivity.HasValue)
            {
                if (now - _lastActivity.Value >= _graceSeconds)
                    _isComplete = true;
                return;
            }

            if (!_wasReady)
            {
                //Ready with nothing pending: the grace window starts now.
                _readySince = now;
                _wasReady = true;
            }

            if (_readySince.HasValue && now - _readySince.Value >= _graceSeconds)
                _isComplete = true;
        }

        public void Reset()
        {
            _isComplete = false;
            _wasReady = false;
            _readySince = null;
            _lastActivity = null;
        }
    }
}
