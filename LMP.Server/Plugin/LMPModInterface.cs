using LMP.Server.Client;
using LMP.Server.Log;
using System.Collections.Generic;

namespace LMP.Server.Plugin
{
    /// <summary>
    ///     LMP message callback.
    ///     client - The client that has sent the message
    ///     modData - The mod byte[] payload
    /// </summary>
    public delegate void LmpMessageCallback(ClientStructure client, byte[] modData);

    public class LmpModInterface
    {
        private static readonly Dictionary<string, LmpMessageCallback> RegisteredMods =
            new Dictionary<string, LmpMessageCallback>();

        private static readonly object EventLock = new object();

        /// <summary>
        ///     Registers a mod handler function that will be called as soon as the message is received.
        /// </summary>
        /// <param name="modName">Mod Name.</param>
        /// <param name="handlerFunction">Handler function.</param>
        public static bool RegisterModHandler(string modName, LmpMessageCallback handlerFunction)
        {
            lock (EventLock)
            {
                if (RegisteredMods.ContainsKey(modName))
                {
                    LunaLog.Debug($"Failed to register mod handler for {modName}, mod already registered");
                    return false;
                }
                LunaLog.Debug($"Registered mod handler for {modName}");
                RegisteredMods.Add(modName, handlerFunction);
            }
            return true;
        }

        /// <summary>
        ///     Unregisters a mod handler.
        /// </summary>
        /// <param name="modName">Mod Name.</param>
        /// <returns><c>true</c> if a mod handler was unregistered</returns>
        public static bool UnregisterModHandler(string modName)
        {
            var unregistered = false;
            lock (EventLock)
            {
                if (RegisteredMods.ContainsKey(modName))
                {
                    RegisteredMods.Remove(modName);
                    unregistered = true;
                }
            }
            return unregistered;
        }

        /// <summary>
        ///     Internal use only - Called when a mod message is received from ClientHandler.
        /// </summary>
        public static void OnModMessageReceived(ClientStructure client, string modName, byte[] modData)
        {
            lock (EventLock)
            {
                if (RegisteredMods.ContainsKey(modName))
                    RegisteredMods[modName](client, modData);
            }
        }
    }
}