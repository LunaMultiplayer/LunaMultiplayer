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
        public static EventData<PartModule, string, short> onPartModuleShortFieldChanged;
        public static EventData<PartModule, string, ushort> onPartModuleUShortFieldChanged;
        public static EventData<PartModule, string, int> onPartModuleIntFieldChanged;
        public static EventData<PartModule, string, uint> onPartModuleUIntFieldChanged;
        public static EventData<PartModule, string, float> onPartModuleFloatFieldChanged;
        public static EventData<PartModule, string, long> onPartModuleLongFieldChanged;
        public static EventData<PartModule, string, ulong> onPartModuleULongFieldChanged;
        public static EventData<PartModule, string, double> onPartModuleDoubleFieldChanged;
        public static EventData<PartModule, string, Vector2> onPartModuleVector2FieldChanged;
        public static EventData<PartModule, string, Vector3> onPartModuleVector3FieldChanged;
        public static EventData<PartModule, string, Quaternion> onPartModuleQuaternionFieldChanged;
        public static EventData<PartModule, string, object> onPartModuleObjectFieldChanged;
        public static EventData<PartModule, string, int, string> onPartModuleEnumFieldChanged;

        public static EventData<PartModule, string> onPartModuleMethodCalling;
        public static EventData<ProtoPartModuleSnapshot, string> onPartModuleMethodProcessed;

        public static EventData<ProtoPartModuleSnapshot, string, string> onPartModuleStringFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, bool> onPartModuleBoolFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, short> onPartModuleShortFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, ushort> onPartModuleUShortFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, int> onPartModuleIntFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, uint> onPartModuleUIntFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, float> onPartModuleFloatFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, long> onPartModuleLongFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, ulong> onPartModuleULongFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, double> onPartModuleDoubleFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, Vector3> onPartModuleVector2FieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, Vector3> onPartModuleVector3FieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, Quaternion> onPartModuleQuaternionFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, object> onPartModuleObjectFieldProcessed;
        public static EventData<ProtoPartModuleSnapshot, string, int, string> onPartModuleEnumFieldProcessed;
    }
}
