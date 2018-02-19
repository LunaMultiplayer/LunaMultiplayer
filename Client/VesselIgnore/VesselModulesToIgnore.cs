using System.Collections.Generic;

namespace LunaClient.VesselIgnore
{
    /// <summary>
    /// Here we hold a collection of modules and fields that can be ignored when checking for changes
    /// </summary>
    public class VesselModulesToIgnore
    {
        /// <summary>
        /// Add here modules that should be ignored (just for performance) when applying changes and when checking for changes
        /// </summary>
        public static readonly string[] ModulesToIgnore = {
            "CModuleLinkedMesh", "FXModuleAnimateThrottle", "ModuleTripLogger", "ModuleCoreHeat"
        };

        /// <summary>
        /// Add here modules that can be ignored when checking if a part module has changes
        /// </summary>
        public static readonly string[] ModulesToIgnoreWhenChecking = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add here modules that fail when calling "OnAwake" on it's partmodule or are useless and can be skipped
        /// </summary>
        public static readonly string[] ModulesToDontAwake = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add here modules that fail when calling "OnLoad" on it's partmodule or are useless and can be skipped
        /// </summary>
        public static readonly string[] ModulesToDontLoad = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add here modules that fail when calling "OnStart" on it's partmodule or are useless and can be skipped
        /// </summary>
        public static readonly string[] ModulesToDontStart = {
            "ModuleWheelBase", "ModuleWheelSteering", "ModuleWheelSuspension", "ModuleScienceContainer", "KerbalEVA"
        };

        /// <summary>
        /// Add fields of a Module that can be ignored (just for performance) both in checking and in applying
        /// </summary>
        public static readonly Dictionary<string, string[]> FieldsToIgnore = new Dictionary<string, string[]>()
        {
            ["ModuleWheelMotor"] = new[] { "driveLimiter" },
            ["ModuleWheelBrakes"] = new[] { "brakeInput" },
            ["ModuleReactionWheel"] = new[] { "stateString" },
            ["ModuleEngines"] = new[] { "currentThrottle" },
            ["ModuleEnginesFX"] = new[] { "currentThrottle" },
            ["ModuleWheelSuspension"] = new[] { "suspensionPos", "autoBoost" },
            ["ModuleDeployableSolarPanel"] = new[] { "launchUT", "currentRotation", "storedAnimationTime" },
            ["ModuleDeployableRadiator"] = new[] { "currentRotation" },
            ["ModuleLogisticsConsumer"] = new[] { "lastCheck" } //Ignore USI field
        };
    }
}
