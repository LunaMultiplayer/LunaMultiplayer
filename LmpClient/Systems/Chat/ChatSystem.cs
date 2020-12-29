using LmpClient.Base;
using LmpClient.Systems.SettingsSys;
using LmpClient.Windows.Chat;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.Chat
{
    public class ChatSystem : MessageSystem<ChatSystem, ChatMessageSender, ChatMessageHandler>
    {
        #region Fields

        public bool NewMessageReceived { get; set; }
        public bool SendEventHandled { get; set; } = true;

        //State tracking
        public LimitedQueue<Tuple<string, string, string>> ChatMessages { get; private set; }

        #endregion

        #region Properties

        public ConcurrentQueue<Tuple<string, string, string>> NewChatMessages { get; private set; } = new ConcurrentQueue<Tuple<string, string, string>>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ChatSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            ChatMessages = new LimitedQueue<Tuple<string, string, string>>(SettingsSystem.CurrentSettings.ChatBuffer);
            SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, ProcessReceivedMessages));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            NewMessageReceived = false;
            SendEventHandled = true;

            ChatMessages.Clear();
            NewChatMessages = new ConcurrentQueue<Tuple<string, string, string>>();
        }

        #endregion

        #region Update methods

        private void ProcessReceivedMessages()
        {
            if (Enabled)
            {
                while (NewChatMessages.TryDequeue(out var chatMsg))
                {
                    NewMessageReceived = true;

                    if (!ChatWindow.Singleton.Display)
                    {
                        LunaScreenMsg.PostScreenMessage($"{chatMsg.Item1}: {chatMsg.Item2}", 5f, ScreenMessageStyle.UPPER_LEFT);
                    }
                    else
                    {
                        ChatWindow.Singleton.ScrollToBottom();
                    }

                    ChatMessages.Enqueue(chatMsg);
                }
            }
        }

        #endregion

        #region Public

        public void PrintToChat(string text)
        {
            NewChatMessages.Enqueue(new Tuple<string, string, string>(SettingsSystem.ServerSettings.ConsoleIdentifier, text,
                SettingsSystem.ServerSettings.ConsoleIdentifier + ": " + text));
        }

        public void PmMessageServer(string message)
        {
            MessageSender.SendChatMsg(message, false);
        }

        #endregion
    }
}
