using LunaClient.Events.Base;
using UnityEngine;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
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
    }
}
