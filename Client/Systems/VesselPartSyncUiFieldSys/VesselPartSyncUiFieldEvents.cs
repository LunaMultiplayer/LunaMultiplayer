using LunaClient.Base;
using LunaClient.Events;
using LunaClient.ModuleStore;
using UniLinq;

namespace LunaClient.Systems.VesselPartSyncUiFieldSys
{
    public class VesselPartSyncUiFieldEvents : SubSystem<VesselPartSyncUiFieldSystem>
    {
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
                            if (moduleCustomization.Fields.Any(f => f.FieldName != field.name))
                                continue;

                            if (field.uiControlFlight.GetType() != typeof(UI_Toggle) //bool
                                || field.uiControlFlight.GetType() != typeof(UI_FloatRange) //float
                                || field.uiControlFlight.GetType() != typeof(UI_Cycle)) //int
                                continue;

                            field.uiControlFlight.onFieldChanged += OnFieldChanged;
                        }
                    }
                }
            }
        }

        private static void OnFieldChanged(BaseField baseField, object oldValue)
        {
            //TODO: add some sort of buffering so it sneds the value after o,5s to avoid clogging in case a user changes it too often

            var fieldType = baseField.FieldInfo.FieldType;

            if (fieldType == typeof(bool))
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(baseField.host as PartModule, baseField.name, (bool)baseField.GetValue(baseField.host));
            else if (fieldType == typeof(int))
                PartModuleEvent.onPartModuleIntFieldChanged.Fire(baseField.host as PartModule, baseField.name, (int)baseField.GetValue(baseField.host));
            else if(fieldType == typeof(float))
                PartModuleEvent.onPartModuleFloatFieldChanged.Fire(baseField.host as PartModule, baseField.name, (float)baseField.GetValue(baseField.host));
        }
    }
}
