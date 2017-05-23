using System;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat.Command;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Chat;

namespace LunaClient.Systems.Chat
{
    public class ChatSystem : MessageSystem<ChatSystem, ChatMessageSender, ChatMessageHandler>
    {
        #region Fields

        public bool ChatButtonHighlighted { get; set; } = false;
        public bool LeaveEventHandled { get; set; } = true;
        public bool SendEventHandled { get; set; } = true;

        //State tracking
        public Dictionary<string, ChatCommand> RegisteredChatCommands { get; } = new Dictionary<string, ChatCommand>();
        public Dictionary<string, List<string>> ChannelMessages { get; } = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> PrivateMessages { get; } = new Dictionary<string, List<string>>();
        public List<string> ConsoleMessages { get; } = new List<string>();
        public Dictionary<string, List<string>> PlayerChannels { get; } = new Dictionary<string, List<string>>();
        public List<string> JoinedChannels { get; } = new List<string>();
        public List<string> JoinedPmChannels { get; } = new List<string>();
        public List<string> HighlightChannel { get; } = new List<string>();
        public List<string> HighlightPm { get; } = new List<string>();

        public string SelectedChannel { get; set; } = null;
        public string SelectedPmChannel { get; set; } = null;
        public bool ChatLocked { get; set; } = false;
        public bool SelectTextBox { get; set; } = false;
        public string SendText { get; set; } = "";

        //const
        public string LmpChatLock { get; } = "LMP_ChatLock";

        #endregion

        #region Properties
        public ChatQueuer Queuer { get; } = new ChatQueuer();
        public ChatEvents ChatEventHandler { get; } = new ChatEvents();
        public override IInputHandler InputHandler { get; } = new ChatInputHandler();

        #endregion

        #region Constructor

        public ChatSystem()
        {
            RegisterChatCommand("help", HelpCommand.DisplayHelp, "Displays this help");
            RegisterChatCommand("join", JoinCommand.JoinChannel, "Joins a new chat Channel");
            RegisterChatCommand("query", QueryCommand.StartQuery, "Starts a query");
            RegisterChatCommand("leave", LeaveCommand.LeaveChannel, "Leaves the current Channel");
            RegisterChatCommand("part", LeaveCommand.LeaveChannel, "Leaves the current Channel");
            RegisterChatCommand("motd", MotdCommand.ServerMotd, "Gets the current Message of the Day");
            RegisterChatCommand("resize", ResizeCommand.ResizeChat, "Resized the chat window");
            RegisterChatCommand("version", VersionCommand.DisplayVersion, "Displays the current version of LMP");
        }

        #endregion
        
        #region Commands

        private static HelpCommand HelpCommand { get; } = new HelpCommand();
        private static JoinCommand JoinCommand { get; } = new JoinCommand();
        private static LeaveCommand LeaveCommand { get; } = new LeaveCommand();
        private static MotdCommand MotdCommand { get; } = new MotdCommand();
        private static QueryCommand QueryCommand { get; } = new QueryCommand();
        private static ResizeCommand ResizeCommand { get; } = new ResizeCommand();
        private static VersionCommand VersionCommand { get; } = new VersionCommand();

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, HandleChatEvents));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            RegisteredChatCommands.Clear();
            ChatButtonHighlighted = false;
            LeaveEventHandled = true;
            SendEventHandled = true;

            RegisteredChatCommands.Clear();
            ChannelMessages.Clear();
            PrivateMessages.Clear();
            ConsoleMessages.Clear();
            PlayerChannels.Clear();
            JoinedChannels.Clear();
            JoinedPmChannels.Clear();
            HighlightChannel.Clear();
            HighlightPm.Clear();

            SelectedChannel = null;
            SelectedPmChannel = null;
            
            SelectTextBox = false;
            SendText = "";

            Queuer.Clear();

            if (ChatLocked)
            {
                InputLockManager.RemoveControlLock(LmpChatLock);
                ChatLocked = false;
            }
        }

        #endregion

        #region Update methods

        private void HandleChatEvents()
        {
            if (Enabled)
            {
                ChatEventHandler.HandleChatEvents();
            }
        }
        
        #endregion

        #region Public

        public void PrintToSelectedChannel(string text)
        {
            if ((SelectedChannel == null) && (SelectedPmChannel == null))
                Queuer.QueueChannelMessage(SettingsSystem.CurrentSettings.PlayerName, "", text);
            if ((SelectedChannel != null) && (SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier))
                Queuer.QueueChannelMessage(SettingsSystem.CurrentSettings.PlayerName, SelectedChannel, text);
            if (SelectedChannel == SettingsSystem.ServerSettings.ConsoleIdentifier)
                Queuer.QueueSystemMessage(text);
            if (SelectedPmChannel != null)
                Queuer.QueuePrivateMessage(SettingsSystem.CurrentSettings.PlayerName, SelectedPmChannel, text);
        }

        public void PmMessageServer(string message)
        {
            MessageSender.SendMessage(new ChatPrivateMsgData
            {
                From = SettingsSystem.CurrentSettings.PlayerName,
                Text = message,
                To = SettingsSystem.ServerSettings.ConsoleIdentifier
            });
        }

        #endregion

        #region Private

        private void RegisterChatCommand(string command, Action<string> func, string description)
        {
            var cmd = new ChatCommand(command, func, description);
            if (!RegisteredChatCommands.ContainsKey(command))
                RegisteredChatCommands.Add(command, cmd);
        }

        #endregion
    }
}