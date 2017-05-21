using System.Diagnostics;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message;

namespace LunaClient.Base
{
    /// <summary>
    /// Subsystem and system base class, it has a message factory to make message handling easier
    /// </summary>
    public abstract class SystemBase
    {
        public static ClientMessageFactory MessageFactory { get; } = new ClientMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);

        /// <summary>
        /// Use this timer to check when to send messages or as a substitute of coroutines
        /// </summary>
        protected Stopwatch Timer { get; } = Stopwatch.StartNew();

        protected void ResetTimer()
        {
            Timer.Reset();
            Timer.Start();
        }
    }
}