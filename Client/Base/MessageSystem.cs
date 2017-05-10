using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Interface;
using Debug = UnityEngine.Debug;

namespace LunaClient.Base
{
    public abstract class MessageSystem<T, TS, TH> : System<T>
        where T : class, ISystem, new()
        where TS : class, IMessageSender, new()
        where TH : class, IMessageHandler, new()
    {
        protected virtual bool HandleMessagesInFixedUpdate { get; set; }

        public TS MessageSender { get; } = new TS();
        public TH MessageHandler { get; } = new TH();
        public virtual IInputHandler InputHandler { get; } = null;
        
        public virtual void EnqueueMessage(IMessageData msg)
        {
            if (Enabled)
                MessageHandler.IncomingMessages.Enqueue(msg);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            //Clear the message queue on disabling
            MessageHandler.IncomingMessages = new ConcurrentQueue<IMessageData>();
        }

        /// <summary>
        /// During the update we receive messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (!HandleMessagesInFixedUpdate)
            {
                ReadAndHandleAllReceivedMessages();
            }
        }

        /// <summary>
        /// During the update we receive messages.
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (HandleMessagesInFixedUpdate)
            {
                ReadAndHandleAllReceivedMessages();
            }
        }

        /// <summary>
        /// Reads all the message queue and calls the handling sub-system
        /// </summary>
        private void ReadAndHandleAllReceivedMessages()
        {
            IMessageData msgData;
            while (MessageHandler.IncomingMessages.TryDequeue(out msgData))
            {
                try
                {
                    MessageHandler.HandleMessage(msgData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Error handling Message type {msgData.GetType()}, exception: {e}");
                    NetworkConnection.Disconnect($"Error handling {msgData.GetType()} Message");
                }
            }
        }
    }
}