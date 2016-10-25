using System;
using System.Collections.Concurrent;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Base
{
    public abstract class MessageSystem<T, TS, TH> : System<T>
        where T : class, ISystem, new()
        where TS : class, IMessageSender, new()
        where TH : class, IMessageHandler, new()
    {
        public TS MessageSender { get; } = new TS();
        public TH MessageHandler { get; } = new TH();
        public virtual IInputHandler InputHandler { get; } = null;

        public void ClearIncomingMsgQueue()
        {
            MessageHandler.IncomingMessages = new ConcurrentQueue<IMessageData>();
        }

        public virtual void EnqueueMessage(IMessageData msg)
        {
            MessageHandler.IncomingMessages.Enqueue(msg);
        }

        /// <summary>
        /// During the fixed update we receive messages.
        /// We do it here as fixedUpdate can be called several times per frame 
        /// so when we reach Update we may have more messages
        /// </summary>
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            IMessageData msgData;
            while (MessageHandler.IncomingMessages.TryDequeue(out msgData))
            {
                try
                {
                    MessageHandler.HandleMessage(msgData);
                }
                catch (Exception e)
                {
                    Debug.LogError("[LMP]: Error handling Message type " + msgData.GetType() + ", exception: " + e);
                    NetworkConnection.Disconnect("Error handling " + msgData.GetType() + " Message");
                }
            }
        }
    }
}