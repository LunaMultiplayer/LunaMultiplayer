using System.Collections.Concurrent;
using System.Collections.Generic;
using LunaClient.Base;
using LunaCommon.Message.Data;
using UnityEngine;

namespace LunaClient.Systems.ModApi
{
    public class ModApiSystem : MessageSystem<ModApiSystem, ModApiMessageSender, ModApiMessageHandler>
    {
        #region Fields

        public Dictionary<string, MessageCallback> RegisteredRawMods { get; } =
            new Dictionary<string, MessageCallback>();

        public Dictionary<string, ConcurrentQueue<byte[]>> UpdateQueue { get; } =
            new Dictionary<string, ConcurrentQueue<byte[]>>();

        public Dictionary<string, ConcurrentQueue<byte[]>> FixedUpdateQueue { get; } =
            new Dictionary<string, ConcurrentQueue<byte[]>>();

        public object EventLock { get; } = new object();

        private Dictionary<string, MessageCallback> RegisteredUpdateMods { get; } =
            new Dictionary<string, MessageCallback>();

        private Dictionary<string, MessageCallback> RegisteredFixedUpdateMods { get; } =
            new Dictionary<string, MessageCallback>();

        #endregion

        #region Constructor

        public ModApiSystem()
        {
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ModApiUpdate));
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, ModApiFixedUpdate));
        }

        #endregion

        #region Base overrides

        public override bool Enabled => true;
        
        #endregion

        #region Update methods

        private void ModApiUpdate()
        {
            lock (EventLock)
            {
                foreach (var currentModQueue in UpdateQueue)
                {
                    byte[] value;
                    while (currentModQueue.Value.TryDequeue(out value))
                        RegisteredUpdateMods[currentModQueue.Key](value);
                }
            }
        }

        #endregion

        #region Fixed update methods

        private void ModApiFixedUpdate()
        {
            lock (EventLock)
            {
                foreach (var currentModQueue in FixedUpdateQueue)
                {
                    byte[] value;
                    while (currentModQueue.Value.TryDequeue(out value))
                        RegisteredFixedUpdateMods[currentModQueue.Key](value);
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Unregisters a mod handler.
        /// </summary>
        /// <returns><c>true</c>, if mod handler was unregistered, <c>false</c> otherwise.</returns>
        /// <param name="modName">Mod Name.</param>
        public bool UnregisterModHandler(string modName)
        {
            var unregistered = false;
            lock (EventLock)
            {
                if (RegisteredRawMods.ContainsKey(modName))
                {
                    RegisteredRawMods.Remove(modName);
                    unregistered = true;
                }
                if (RegisteredUpdateMods.ContainsKey(modName))
                {
                    RegisteredUpdateMods.Remove(modName);
                    UpdateQueue.Remove(modName);
                    unregistered = true;
                }
                if (RegisteredFixedUpdateMods.ContainsKey(modName))
                {
                    RegisteredFixedUpdateMods.Remove(modName);
                    FixedUpdateQueue.Remove(modName);
                    unregistered = true;
                }
            }
            return unregistered;
        }

        /// <summary>
        ///     Registers a mod handler function that will be called as soon as the Message is received.
        ///     This is called from the networking thread, so you should avoid interacting with KSP directly here as Unity is not
        ///     thread safe.
        /// </summary>
        /// <param name="modName">Mod Name.</param>
        /// <param name="handlerFunction">Handler function.</param>
        public bool RegisterRawModHandler(string modName, MessageCallback handlerFunction)
        {
            lock (EventLock)
            {
                if (RegisteredRawMods.ContainsKey(modName))
                {
                    Debug.Log($"[LMP]: Failed to register raw mod handler for {modName}, mod already registered");
                    return false;
                }
                Debug.Log($"[LMP]: Registered raw mod handler for {modName}");
                RegisteredRawMods.Add(modName, handlerFunction);
            }
            return true;
        }

        /// <summary>
        ///     Registers a mod handler function that will be called on every Update.
        /// </summary>
        /// <param name="modName">Mod Name.</param>
        /// <param name="handlerFunction">Handler function.</param>
        public bool RegisterUpdateModHandler(string modName, MessageCallback handlerFunction)
        {
            lock (EventLock)
            {
                if (RegisteredUpdateMods.ContainsKey(modName))
                {
                    Debug.Log($"[LMP]: Failed to register Update mod handler for {modName}, mod already registered");
                    return false;
                }
                Debug.Log($"[LMP]: Registered Update mod handler for {modName}");
                RegisteredUpdateMods.Add(modName, handlerFunction);
                UpdateQueue.Add(modName, new ConcurrentQueue<byte[]>());
            }
            return true;
        }

        /// <summary>
        ///     Registers a mod handler function that will be called on every FixedUpdate.
        /// </summary>
        /// <param name="modName">Mod Name.</param>
        /// <param name="handlerFunction">Handler function.</param>
        public bool RegisterFixedUpdateModHandler(string modName, MessageCallback handlerFunction)
        {
            lock (EventLock)
            {
                if (RegisteredFixedUpdateMods.ContainsKey(modName))
                {
                    Debug.Log($"[LMP]: Failed to register FixedUpdate mod handler for {modName}, mod already registered");
                    return false;
                }
                Debug.Log($"[LMP]: Registered FixedUpdate mod handler for {modName}");
                RegisteredFixedUpdateMods.Add(modName, handlerFunction);
                FixedUpdateQueue.Add(modName, new ConcurrentQueue<byte[]>());
            }
            return true;
        }

        /// <summary>
        ///     Sends a mod Message.
        /// </summary>
        /// <param name="modName">Mod Name</param>
        /// <param name="messageData">The Message payload (MessageWriter can make this easier)</param>
        /// <param name="relay">If set to <c>true</c>, The server will relay the Message to all other authenticated clients</param>
        public void SendModMessage(string modName, byte[] messageData, bool relay)
        {
            if (modName == null)
                return;
            if (messageData == null)
            {
                Debug.LogError($"[LMP]: {modName} attemped to send a null Message");
                return;
            }
            MessageSender.SendMessage(new ModMsgData {Data = messageData, Relay = relay, ModName = modName});
        }

        #endregion
    }
}