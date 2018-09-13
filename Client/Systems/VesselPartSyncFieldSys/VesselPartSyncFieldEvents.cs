using LunaClient.Base;
using LunaClient.ModuleStore;
using System;
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
            System.MessageSender.SendVesselPartSyncFieldBoolMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleIntFieldChanged(PartModule module, string fieldName, int newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new INT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldIntMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleFloatFieldChanged(PartModule module, string fieldName, float newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new FLOAT value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldFloatMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleDoubleFieldChanged(PartModule module, string fieldName, double newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new DOUBLE value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldDoubleMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleVectorFieldChanged(PartModule module, string fieldName, Vector3 newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new VECTOR value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldVectorMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleQuaternionFieldChanged(PartModule module, string fieldName, Quaternion newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new QUATERNION value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldQuaternionMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleStringFieldChanged(PartModule module, string fieldName, string newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new STRING value of {newValue}.");
            System.MessageSender.SendVesselPartSyncFieldStringMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        public void PartModuleObjectFieldChanged(PartModule module, string fieldName, object newValue)
        {
            if (!CallIsValid(module))
                return;

            LunaLog.Log($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a new OBJECT value of {newValue}.");
            LunaLog.LogWarning($"Field {fieldName} in module {module.moduleName} from part {module.part.flightID} has a field type that is not supported!");
            System.MessageSender.SendVesselPartSyncFieldObjectMsg(module.vessel.id, module.part.flightID, module.moduleName, fieldName, newValue);
        }

        #endregion

        public void PartModuleFieldChanged(PartModule module, string fieldName)
        {
            if (!CallIsValid(module))
                return;

            var fieldVal = VesselPartModuleAccess.GetPartModuleFieldValue(module.vessel.id, module.part.flightID, module.moduleName, fieldName);
            if (fieldVal == null) return;

            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        customization.GetCustomizationForField(fieldName);
                    }
                }
            }

        }

        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            if (FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == Guid.Empty)
                return;

            VesselPartModuleAccess.AddVessel(FlightGlobals.ActiveVessel);
        }

        public void VesselLoaded(Vessel vessel)
        {
            VesselPartModuleAccess.AddVessel(vessel);
        }

        public void VesselPartCountChanged(Vessel vessel)
        {
            VesselPartModuleAccess.UpdateVessel(vessel);
        }

        public void VesselUnloaded(Vessel vessel)
        {
            VesselPartModuleAccess.RemoveVessel(vessel.id);
        }
    }
}
