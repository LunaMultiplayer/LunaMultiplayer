using LmpClient.Base;
using LmpClient.Events;
using LmpClient.ModuleStore;

namespace LmpClient.Systems.VesselPartSyncUiFieldSys
{
    public class VesselPartSyncUiFieldEvents : SubSystem<VesselPartSyncUiFieldSystem>
    {
        private static bool CallIsValid(PartModule module)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null)
                return false;

            var part = module.part;
            if (part == null)
                return false;

            //The vessel is immortal so we are sure that it's not ours
            if (float.IsPositiveInfinity(part.crashTolerance))
                return false;

            return true;
        }

        public void SubscribeToFieldChanges()
        {
            SubscribeToFieldChanges(FlightGlobals.ActiveVessel);
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
                            if (field.uiControlFlight.GetType() != typeof(UI_Toggle) //bool
                                && field.uiControlFlight.GetType() != typeof(UI_FloatRange) //float
                                && field.uiControlFlight.GetType() != typeof(UI_Cycle)) //int
                                continue;

                            field.uiControlFlight.onFieldChanged -= OnFieldChanged;
                            field.uiControlFlight.onFieldChanged += OnFieldChanged;
                        }
                    }
                }
            }
        }

        private static void OnFieldChanged(BaseField baseField, object oldValue)
        {
            if (!CallIsValid((PartModule)baseField.host)) return;

            //TODO: add some sort of buffering so it sends the value after 500ms to avoid clogging in case a user changes it too often

            var fieldType = baseField.FieldInfo.FieldType;

            if (fieldType == typeof(bool))
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire((PartModule)baseField.host, baseField.name, (bool)baseField.GetValue(baseField.host));
            else if (fieldType == typeof(int))
                PartModuleEvent.onPartModuleIntFieldChanged.Fire((PartModule)baseField.host, baseField.name, (int)baseField.GetValue(baseField.host));
            else if (fieldType == typeof(float))
                PartModuleEvent.onPartModuleFloatFieldChanged.Fire((PartModule)baseField.host, baseField.name, (float)baseField.GetValue(baseField.host));
        }
    }
}
