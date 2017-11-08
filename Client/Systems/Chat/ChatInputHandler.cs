using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Chat;
using System;

namespace LunaClient.Systems.Chat
{
    public class ChatInputHandler : SubSystem<ChatSystem>, IInputHandler
    {
        public void HandleInput(string input)
        {
            if (!input.StartsWith("/") || input.StartsWith("//"))
            {
                //Handle chat messages
                if (input.StartsWith("//"))
                    input = input.Substring(1);

                if (System.SelectedChannel == null && System.SelectedPmChannel == null)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ChatChannelMsgData>();
                    msgData.From = SettingsSystem.CurrentSettings.PlayerName;
                    msgData.Channel = string.Empty;
                    msgData.SendToAll = true;
                    msgData.Text = input;

                    System.MessageSender.SendMessage(msgData);
                }
                if (System.SelectedChannel != null && System.SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ChatChannelMsgData>();
                    msgData.From = SettingsSystem.CurrentSettings.PlayerName;
                    msgData.Channel = System.SelectedChannel;
                    msgData.SendToAll = false;
                    msgData.Text = input;

                    System.MessageSender.SendMessage(msgData);
                }
                if (System.SelectedChannel == SettingsSystem.ServerSettings.ConsoleIdentifier)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ChatConsoleMsgData>();
                    msgData.From = SettingsSystem.CurrentSettings.PlayerName;
                    msgData.Message = input;

                    System.MessageSender.SendMessage(msgData);

                    LunaLog.Log($"[LMP]: Server Command: {input}");
                }
                if (System.SelectedPmChannel != null)
                {
                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ChatPrivateMsgData>();
                    msgData.From = SettingsSystem.CurrentSettings.PlayerName;
                    msgData.Text = input;
                    msgData.To = System.SelectedPmChannel;

                    System.MessageSender.SendMessage(msgData);
                }
            }
            else
            {
                var commandPart = input.Substring(1);
                var argumentPart = "";
                if (commandPart.Contains(" "))
                {
                    if (commandPart.Length > commandPart.IndexOf(' ') + 1)
                        argumentPart = commandPart.Substring(commandPart.IndexOf(' ') + 1);
                    commandPart = commandPart.Substring(0, commandPart.IndexOf(' '));
                }
                if (commandPart.Length > 0)
                    if (System.RegisteredChatCommands.ContainsKey(commandPart))
                        try
                        {
                            LunaLog.Log($"[LMP]: Chat Command: {input.Substring(1)}");
                            System.RegisteredChatCommands[commandPart].Func(argumentPart);
                        }
                        catch (Exception e)
                        {
                            LunaLog.LogError($"[LMP]: Error handling chat command {commandPart}, Exception {e}");
                            System.PrintToSelectedChannel($"Error handling chat command: {commandPart}");
                        }
                    else
                        System.PrintToSelectedChannel($"Unknown chat command: {commandPart}");
            }
        }
    }
}