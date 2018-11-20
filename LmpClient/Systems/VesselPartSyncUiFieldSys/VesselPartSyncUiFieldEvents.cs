using LmpClient.Base;
using LmpClient.Extensions;
using LmpClient.ModuleStore;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Locks;

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
            var partModule = (PartModule)baseField.host;
            if (!CallIsValid(partModule)) return;

            //TODO: add some sort of buffering so it sends the value after 500ms to avoid clogging in case a user changes it too often

            var fieldType = baseField.FieldInfo.FieldType;

            if (fieldType == typeof(bool))
                System.MessageSender.SendVesselPartSyncUiFieldBoolMsg(partModule.vessel, partModule.part, partModule.moduleName, baseField.name, (bool)baseField.GetValue(baseField.host));
            else if (fieldType == typeof(int))
                System.MessageSender.SendVesselPartSyncUiFieldIntMsg(partModule.vessel, partModule.part, partModule.moduleName, baseField.name, (int)baseField.GetValue(baseField.host));
            else if (fieldType == typeof(float))
                System.MessageSender.SendVesselPartSyncUiFieldFloatMsg(partModule.vessel, partModule.part, partModule.moduleName, baseField.name, (float)baseField.GetValue(baseField.host));
        }
    }
}
