using LunaClient.Base;
using UnityEngine;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    public class VesselPartSyncFieldEvents : SubSystem<VesselPartSyncFieldSystem>
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

        #region PartField change events

        public void PartModuleBoolFieldChanged(PartModule module, string fieldName, bool newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new BOOL value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldBoolMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleIntFieldChanged(PartModule module, string fieldName, int newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new INT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldIntMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleFloatFieldChanged(PartModule module, string fieldName, float newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new FLOAT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldFloatMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleDoubleFieldChanged(PartModule module, string fieldName, double newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new DOUBLE value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldDoubleMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleVectorFieldChanged(PartModule module, string fieldName, Vector3 newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new VECTOR value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldVectorMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleQuaternionFieldChanged(PartModule module, string fieldName, Quaternion newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new QUATERNION value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldQuaternionMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleStringFieldChanged(PartModule module, string fieldName, string newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new STRING value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldStringMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleObjectFieldChanged(PartModule module, string fieldName, object newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new OBJECT value of {newValue}.");
            LunaLog.LogWarning($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a field type that is not supported!");
            System.MessageSender.SendVesselPartSyncFieldObjectMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }
        
        public void PartModuleEnumFieldChanged(PartModule module, string fieldName, int newValue, string newValueStr)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new ENUM value of {newValueStr}.");
            System.MessageSender.SendVesselPartSyncFieldEnumMsg(module.vessel, module.part, module.moduleName, fieldName, newValue, newValueStr);
        }

        #endregion
    }
}
