using LmpClient.Base;
using LmpClient.Extensions;
using LmpClient.ModuleStore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LmpClient.Systems.VesselPartSyncFieldSys
{
    public class VesselPartSyncFieldEvents : SubSystem<VesselPartSyncFieldSystem>
    {
        private static readonly Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, TimeToSend>>>> LastSendTimeDictionary =
            new Dictionary<Guid, Dictionary<uint, Dictionary<string, Dictionary<string, TimeToSend>>>>();

        private static bool CallIsValid(PartModule module, string fieldName)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null)
                return false;

            var part = module.part;
            if (part == null)
                return false;

            //The vessel is immortal so we are sure that it's not ours
            if (part.vessel.IsImmortal())
                return false;

            if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(module.moduleName, out var customization))
            {
                if (customization.CustomizedFields.TryGetValue(fieldName, out var fieldCust))
                {
                    var timeToSend = LastSendTimeDictionary.GetOrAdd(module.vessel.id, () => new Dictionary<uint, Dictionary<string, Dictionary<string, TimeToSend>>>())
                        .GetOrAdd(module.part.flightID, () => new Dictionary<string, Dictionary<string, TimeToSend>>())
                        .GetOrAdd(module.moduleName, () => new Dictionary<string, TimeToSend>())
                        .GetOrAdd(fieldName, () => new TimeToSend(fieldCust.MaxIntervalInMs));

                    return timeToSend.ReadyToSend();
                }
            }

            return true;
        }

        #region PartField change events

        public void PartModuleBoolFieldChanged(PartModule module, string fieldName, bool newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new BOOL value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldBoolMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }


        public void PartModuleShortFieldChanged(PartModule module, string fieldName, short newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new SHORT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldShortMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleUshortFieldChanged(PartModule module, string fieldName, ushort newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new USHORT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldUshortMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleIntFieldChanged(PartModule module, string fieldName, int newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new INT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldIntMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleUintFieldChanged(PartModule module, string fieldName, uint newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new UINT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldUIntMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleFloatFieldChanged(PartModule module, string fieldName, float newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new FLOAT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldFloatMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }


        public void PartModuleLongFieldChanged(PartModule module, string fieldName, long newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new LONG value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldLongMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleUlongFieldChanged(PartModule module, string fieldName, ulong newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new ULONG value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldULongMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleDoubleFieldChanged(PartModule module, string fieldName, double newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new DOUBLE value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldDoubleMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleVector2FieldChanged(PartModule module, string fieldName, Vector2 newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new VECTOR2 value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldVector2Msg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleVector3FieldChanged(PartModule module, string fieldName, Vector3 newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new VECTOR3 value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldVector3Msg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleQuaternionFieldChanged(PartModule module, string fieldName, Quaternion newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new QUATERNION value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldQuaternionMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleStringFieldChanged(PartModule module, string fieldName, string newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new STRING value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldStringMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleObjectFieldChanged(PartModule module, string fieldName, object newValue)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new OBJECT value of {newValue}.");
            LunaLog.LogWarning($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a field type that is not supported!");
            System.MessageSender.SendVesselPartSyncFieldObjectMsg(module.vessel, module.part, module.moduleName, fieldName, newValue);
        }

        public void PartModuleEnumFieldChanged(PartModule module, string fieldName, int newValue, string newValueStr)
        {
            if (!CallIsValid(module, fieldName))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new ENUM value of {newValueStr}.");
            System.MessageSender.SendVesselPartSyncFieldEnumMsg(module.vessel, module.part, module.moduleName, fieldName, newValue, newValueStr);
        }

        #endregion
    }
}
