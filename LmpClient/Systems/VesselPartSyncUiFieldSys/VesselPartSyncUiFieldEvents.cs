using LmpClient.Base;
using LmpClient.Extensions;
using LmpClient.ModuleStore;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Locks;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselPartSyncUiFieldSys
{
    public class VesselPartSyncUiFieldEvents : SubSystem<VesselPartSyncUiFieldSystem>
    {
        private const int DebounceMs = 200;

        private class PendingChange
        {
            public Vessel Vessel;
            public Part Part;
            public string ModuleName;
            public string FieldName;
            public object Value;
            public Type FieldType;
            public DateTime LastChangedAt;
        }

        private readonly ConcurrentDictionary<string, PendingChange> _pending
            = new ConcurrentDictionary<string, PendingChange>();

        private static bool CallIsValid(PartModule module)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null)
                return false;

            var part = module.part;
            if (part == null)
                return false;

            if (module.vessel.IsImmortal())
                return false;

            return true;
        }

        public void LockAcquire(LockDefinition lockDef)
        {
            if (lockDef.Type == LockType.Control && lockDef.PlayerName == SettingsSystem.CurrentSettings.PlayerName)
            {
                SubscribeToFieldChanges(FlightGlobals.ActiveVessel);
            }
        }

        public void SubscribeToFieldChanges(Vessel vessel)
        {
            foreach (var part in vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(module.moduleName, out var moduleCustomization))
                    {
                        foreach (var field in module.Fields)
                        {
                            if (field.uiControlFlight.GetType() != typeof(UI_Toggle)
                                && field.uiControlFlight.GetType() != typeof(UI_FloatRange)
                                && field.uiControlFlight.GetType() != typeof(UI_Cycle))
                                continue;

                            field.uiControlFlight.onFieldChanged -= OnFieldChanged;
                            field.uiControlFlight.onFieldChanged += OnFieldChanged;
                        }
                    }
                }
            }
        }

        private void OnFieldChanged(BaseField baseField, object oldValue)
        {
            var partModule = (PartModule)baseField.host;
            if (!CallIsValid(partModule)) return;

            var key = $"{partModule.part.flightID}_{partModule.moduleName}_{baseField.name}";
            _pending[key] = new PendingChange
            {
                Vessel = partModule.vessel,
                Part = partModule.part,
                ModuleName = partModule.moduleName,
                FieldName = baseField.name,
                Value = baseField.GetValue(baseField.host),
                FieldType = baseField.FieldInfo.FieldType,
                LastChangedAt = LunaComputerTime.UtcNow
            };
        }

        /// <summary>
        /// Flushes pending outgoing UI field changes that have been stable for DebounceMs.
        /// Called by the system's update routine.
        /// </summary>
        public void FlushPending()
        {
            if (_pending.IsEmpty) return;

            var cutoff = LunaComputerTime.UtcNow.AddMilliseconds(-DebounceMs);
            foreach (var kvp in _pending)
            {
                if (kvp.Value.LastChangedAt > cutoff) continue;

                if (!_pending.TryRemove(kvp.Key, out var change)) continue;

                SendChange(change);
            }
        }

        public void ClearPending() => _pending.Clear();

        private void SendChange(PendingChange change)
        {
            if (change.FieldType == typeof(bool))
                System.MessageSender.SendVesselPartSyncUiFieldBoolMsg(change.Vessel, change.Part, change.ModuleName, change.FieldName, (bool)change.Value);
            else if (change.FieldType == typeof(int))
                System.MessageSender.SendVesselPartSyncUiFieldIntMsg(change.Vessel, change.Part, change.ModuleName, change.FieldName, (int)change.Value);
            else if (change.FieldType == typeof(float))
                System.MessageSender.SendVesselPartSyncUiFieldFloatMsg(change.Vessel, change.Part, change.ModuleName, change.FieldName, (float)change.Value);
        }
    }
}
