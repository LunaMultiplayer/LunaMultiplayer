using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Chat;
using System;
using UnityEngine;

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

                if ((System.SelectedChannel == null) && (System.SelectedPmChannel == null))
                    System.MessageSender.SendMessage(new ChatChannelMsgData
                    {
                        From = SettingsSystem.CurrentSettings.PlayerName,
                        Channel = "",
                        SendToAll = true,
                        Text = input
                    });
                if ((System.SelectedChannel != null) && (System.SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier))
                    System.MessageSender.SendMessage(new ChatChannelMsgData
                    {
                        From = SettingsSystem.CurrentSettings.PlayerName,
                        Channel = System.SelectedChannel,
                        SendToAll = false,
                        Text = input
                    });
                if (System.SelectedChannel == SettingsSystem.ServerSettings.ConsoleIdentifier)
                {
                    System.MessageSender.SendMessage(new ChatConsoleMsgData
                    {
                        From = SettingsSystem.CurrentSettings.PlayerName,
                        Message = input
                    });
                    Debug.Log("[LMP]: Server Command: " + input);
                }
                if (System.SelectedPmChannel != null)
                    System.MessageSender.SendMessage(new ChatPrivateMsgData
                    {
                        From = SettingsSystem.CurrentSettings.PlayerName,
                        Text = input,
                        To = System.SelectedPmChannel
                    });
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
                            Debug.Log("[LMP]: Chat Command: " + input.Substring(1));
                            System.RegisteredChatCommands[commandPart].Func(argumentPart);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("[LMP]: Error handling chat command " + commandPart + ", Exception " + e);
                            System.PrintToSelectedChannel("Error handling chat command: " + commandPart);
                        }
                    else
                        System.PrintToSelectedChannel("Unknown chat command: " + commandPart);
            }
        }
    }
}