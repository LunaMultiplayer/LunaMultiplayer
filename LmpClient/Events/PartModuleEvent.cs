using LmpClient.Events.Base;
using UnityEngine;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class PartModuleEvent : LmpBaseEvent
    {
        public static EventData<PartModule, string, string> onPartModuleStringFieldChanged;
        public static EventData<PartModule, string, bool> onPartModuleBoolFieldChanged;
        public static EventData<PartModule, string, int> onPartModuleIntFieldChanged;
        public static EventData<PartModule, string, float> onPartModuleFloatFieldChanged;
        public static EventData<PartModule, string, double> onPartModuleDoubleFieldChanged;
        public static EventData<PartModule, string, Vector3> onPartModuleVectorFieldChanged;
        public static EventData<PartModule, string, Quaternion> onPartModuleQuaternionFieldChanged;
        public static EventData<PartModule, string, object> onPartModuleObjectFieldChanged;
        public static EventData<PartModule, string, int, string> onPartModuleEnumFieldChanged;

        public static EventData<PartModule, string> onPartModuleMethodCalling;
        public static EventData<ProtoPartModuleSnapshot, string> onPartModuleMethodProcessed;

        public static EventData<ProtoPartModuleSnapshot, string, string> onPartModuleStringFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, bool> onPartModuleBoolFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, int> onPartModuleIntFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, float> onPartModuleFloatFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, double> onPartModuleDoubleFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, Vector3> onPartModuleVectorFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, Quaternion> onPartModuleQuaternionFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, object> onPartModuleObjectFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, int, string> onPartModuleEnumFieldProcessed;
    }
}
