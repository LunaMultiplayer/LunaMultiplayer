using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LmpClient.Systems.VesselProtoSys
{
    public class TransientVesselSendHandler
    {
        private const float RetryDelaySeconds = 1f;

        private const int MaxRetries = 30;

        private class DeferredVesselState
        {
            public string VesselName { get; set; }
            public bool ForceReload { get; set; }
            public string Reason { get; set; }
            public int RetryCount { get; set; }
        }

        public class DeferResult
        {
            public bool Deferred { get; set; }

            public bool EffectiveForceReload { get; set; }

            public string EffectiveReason { get; set; }
        }

        private readonly Action<ProtoVessel, bool, string> _sendAction;

        private readonly ConcurrentDictionary<Guid, DeferredVesselState> _deferredVessels = new ConcurrentDictionary<Guid, DeferredVesselState>();

        public TransientVesselSendHandler(Action<ProtoVessel, bool, string> sendAction)
        {
            _sendAction = sendAction;
        }

        public void Clear()
        {
            _deferredVessels.Clear();
        }

        public DeferResult DeferIfTransient(ProtoVessel protoVessel, bool forceReload, string reason)
        {
            var result = new DeferResult
            {
                EffectiveForceReload = forceReload,
                EffectiveReason = reason,
            };

            if (protoVessel == null) return result;

            //Skip vessels that cannot carry ground state.
            if (!HasGroundModule(protoVessel)) return result;

            if (!IsTransient(protoVessel))
            {
                //Settled with a pending deferred send: consume it now so the retry loop
                //cannot fire a duplicate on the now-settled vessel.
                if (_deferredVessels.TryRemove(protoVessel.vesselID, out var pending))
                {
                    result.EffectiveForceReload = forceReload || pending.ForceReload;
                    //Keep the pending reason — it describes the original send that triggered the deferral.
                    result.EffectiveReason = pending.Reason ?? reason;
                }
                return result;
            }

            //Already deferred: merge into the pending entry instead of a second retry loop.
            if (_deferredVessels.TryGetValue(protoVessel.vesselID, out var existing))
            {
                existing.ForceReload |= forceReload;
                result.Deferred = true;
                return result;
            }

            _deferredVessels[protoVessel.vesselID] = new DeferredVesselState
            {
                VesselName = protoVessel.vesselName,
                ForceReload = forceReload,
                Reason = reason,
            };

            ScheduleRetry(protoVessel.vesselID);

            result.Deferred = true;
            return result;
        }

        #region Private methods

        private static bool HasGroundModule(ProtoVessel protoVessel)
        {
            if (protoVessel.protoPartSnapshots == null) return false;

            foreach (var part in protoVessel.protoPartSnapshots)
            {
                if (part?.modules == null) continue;

                for (var i = 0; i < part.modules.Count; i++)
                {
                    if (TransientDeployedStateDetector.IsGroundModule(part.modules[i].moduleName))
                        return true;
                }
            }

            return false;
        }

        private static bool IsTransient(ProtoVessel protoVessel)
        {
            try
            {
                foreach (var part in protoVessel.protoPartSnapshots)
                {
                    if (part?.modules == null) continue;

                    for (var i = 0; i < part.modules.Count; i++)
                    {
                        var module = part.modules[i];
                        if (!TransientDeployedStateDetector.IsGroundModule(module.moduleName)) continue;

                        var values = module.moduleValues;
                        var beingDeployed = GetPersistedBool(values, "beingDeployed");
                        var deployedOnGround = GetPersistedBool(values, "deployedOnGround");
                        var isEnabled = GetPersistedBool(values, "isEnabled");

                        if (TransientDeployedStateDetector.IsTransientDeployedModule(module.moduleName, beingDeployed, deployedOnGround, isEnabled))
                            return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                // Never block a send just because the detection failed.
                LunaLog.LogError($"[LMP]: Failed to check transient deployed state of vessel {protoVessel.vesselID}: {e}");
                return false;
            }
        }

        private static bool? GetPersistedBool(ConfigNode moduleValues, string key)
        {
            var value = moduleValues?.GetValue(key);
            if (value == null) return null;
            if (bool.TryParse(value, out var result)) return result;
            return null;
        }

        private void ScheduleRetry(Guid vesselId)
        {
            CoroutineUtil.StartDelayedRoutine($"RetryVesselSend_{vesselId}", () => RetrySend(vesselId), RetryDelaySeconds);
        }

        private void RetrySend(Guid vesselId)
        {
            if (!_deferredVessels.TryGetValue(vesselId, out var state)) return;

            var vessel = FlightGlobals.FindVessel(vesselId);
            if (vessel == null || vessel.state == Vessel.State.DEAD || VesselRemoveSystem.Singleton.VesselWillBeKilled(vesselId))
            {
                _deferredVessels.TryRemove(vesselId, out _);
                return;
            }

            if (state.RetryCount >= MaxRetries)
            {
                _deferredVessels.TryRemove(vesselId, out _);
                LunaLog.LogWarning($"[LMP]: Aborted deferred send of vessel {state.VesselName} - {vesselId} - deployment never settled after {MaxRetries} retries");
                return;
            }

            state.RetryCount++;

            ProtoVessel freshProto;
            try
            {
                freshProto = vessel.BackupVessel();
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: BackupVessel failed for deferred send of {vesselId} (will retry): {e.Message}");
                ScheduleRetry(vesselId);
                return;
            }

            if (!IsTransient(freshProto))
            {
                _deferredVessels.TryRemove(vesselId, out _);
                _sendAction?.Invoke(freshProto, state.ForceReload, state.Reason);
                return;
            }

            ScheduleRetry(vesselId);
        }

        #endregion
    }
}
