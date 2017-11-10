using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Base
{
    public abstract class MessageSystem<T, TS, TH> : System
        where T : class, ISystem, new()
        where TS : class, IMessageSender, new()
        where TH : class, IMessageHandler, new()
    {
        /// <summary>
        /// You can set this property to false if your MessageHandler.HandleMessage doesn't call anything 
        /// from Unity and you have your collections as concurrent
        /// </summary>
        protected virtual bool ProcessMessagesInUnityThread => true;

        public TS MessageSender { get; } = new TS();
        public TH MessageHandler { get; } = new TH();
        public virtual IInputHandler InputHandler { get; } = null;

        public virtual void EnqueueMessage(IServerMessageBase msg)
        {
            if (!Enabled) return;

            if (ProcessMessagesInUnityThread)
            {
                MessageHandler.IncomingMessages.Enqueue(msg);
            }
            else
            {
                TaskFactory.StartNew(() =>
                {
                    HandleMessage(msg.Data);
                    msg.Recycle();
                });
            }
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            //Clear the message queue on disabling
            MessageHandler.IncomingMessages = new ConcurrentQueue<IServerMessageBase>();
        }

        protected override void OnEnabled()
        {
            base.OnEnabled();
            //During the update we receive and handle all received messages
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ReadAndHandleAllReceivedMessages));
        }

        /// <summary>
        /// Reads all the message queue and calls the handling sub-system
        /// </summary>
        private void ReadAndHandleAllReceivedMessages()
        {
            if (!ProcessMessagesInUnityThread)
                return;

            while (MessageHandler.IncomingMessages.TryDequeue(out var msg))
            {
                HandleMessage(msg.Data);
                msg.Recycle();
            }
        }

        private void HandleMessage(IMessageData msgData)
        {
            try
            {
                MessageHandler.HandleMessage(msgData);
                //After the message has been handled set it ready to be recycled
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error handling Message type {msgData.GetType()}, exception: {e}");
                NetworkConnection.Disconnect($"Error handling {msgData.GetType()} Message");
            }
        }
    }
}