using LunaClient.Base;
using LunaClient.Network;
using LunaCommon.Message.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LunaClient.Systems.ModApi
{
    public class ModApiSystem : MessageSystem<ModApiSystem, ModApiMessageSender, ModApiMessageHandler>
    {
        #region Fields & properties

        private static bool _enabled = true;

        /// <summary>
        /// This system must be ALWAYS enabled!
        /// </summary>
        public override bool Enabled
        {
            get => _enabled;
            set
            {
                base.Enabled |= value;
                _enabled |= value;
            }
        }

        internal readonly object EventLock = new object();

        public Dictionary<string, MessageCallback> RegisteredRawMods { get; } =
            new Dictionary<string, MessageCallback>();

        public Dictionary<string, ConcurrentQueue<byte[]>> UpdateQueue { get; } =
            new Dictionary<string, ConcurrentQueue<byte[]>>();

        public Dictionary<string, ConcurrentQueue<byte[]>> FixedUpdateQueue { get; } =
            new Dictionary<string, ConcurrentQueue<byte[]>>();

        private Dictionary<string, MessageCallback> RegisteredUpdateMods { get; } =
            new Dictionary<string, MessageCallback>();

        private Dictionary<string, MessageCallback> RegisteredFixedUpdateMods { get; } =
            new Dictionary<string, MessageCallback>();

        #endregion

        #region Constructor

        /// <summary>
        /// This system must be ALWAYS enabled so we set it as enabled on the constructor
        /// </summary>
        public ModApiSystem()
        {
            base.Enabled = true;
        }

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ModApiSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ModApiUpdate));
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, ModApiFixedUpdate));
        }

        public override int ExecutionOrder => int.MinValue + 2;

        #endregion

        #region Update methods

        private void ModApiUpdate()
        {
            lock (EventLock)
            {
                foreach (var currentModQueue in UpdateQueue)
                {
                    while (currentModQueue.Value.TryDequeue(out var value))
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
                    while (currentModQueue.Value.TryDequeue(out var value))
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
                    LunaLog.Log($"[LMP]: Failed to register raw mod handler for {modName}, mod already registered");
                    return false;
                }
                LunaLog.Log($"[LMP]: Registered raw mod handler for {modName}");
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
                    LunaLog.Log($"[LMP]: Failed to register Update mod handler for {modName}, mod already registered");
                    return false;
                }
                LunaLog.Log($"[LMP]: Registered Update mod handler for {modName}");
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
                    LunaLog.Log($"[LMP]: Failed to register FixedUpdate mod handler for {modName}, mod already registered");
                    return false;
                }
                LunaLog.Log($"[LMP]: Registered FixedUpdate mod handler for {modName}");
                RegisteredFixedUpdateMods.Add(modName, handlerFunction);
                FixedUpdateQueue.Add(modName, new ConcurrentQueue<byte[]>());
            }
            return true;
        }

        /// <summary>
        ///     Sends a mod Message.
        /// </summary>
        /// <param name="modName">Mod Name</param>
        /// <param name="messageData">The message payload</param>
        /// <param name="relay">If set to <c>true</c>, The server will relay the Message to all other authenticated clients</param>
        public void SendModMessage(string modName, byte[] messageData, bool relay)
        {
            SendModMessage(modName, messageData, messageData.Length, relay);
        }

        /// <summary>
        ///     Sends a mod Message.
        /// </summary>
        /// <param name="modName">Mod Name</param>
        /// <param name="messageData">The message payload</param>
        /// <param name="numBytes">Number of bytes to take from the array</param>
        /// <param name="relay">If set to <c>true</c>, The server will relay the Message to all other authenticated clients</param>
        public void SendModMessage(string modName, byte[] messageData, int numBytes, bool relay)
        {
            if (modName == null)
                return;
            if (messageData == null)
            {
                LunaLog.LogError($"[LMP]: {modName} attemped to send a null Message");
                return;
            }

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ModMsgData>();

            if (msgData.Data.Length < numBytes)
                msgData.Data = new byte[numBytes];

            Array.Copy(messageData, msgData.Data, numBytes);

            msgData.NumBytes = numBytes;
            msgData.Relay = relay;
            msgData.ModName = modName;

            MessageSender.SendMessage(msgData);
        }

        #endregion
    }
}
