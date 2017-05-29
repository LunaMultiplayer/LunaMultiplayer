using LunaClient.Systems.SettingsSys;
using LunaCommon.Message;

namespace LunaClient.Base
{
    /// <summary>
    /// Subsystem and system base class, it has a message factory to make message handling easier
    /// </summary>
    public abstract class SystemBase
    {
        /// <summary>
        /// Use this property to generate messages
        /// </summary>
        public static ClientMessageFactory MessageFactory { get; } = new ClientMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);
    }
}