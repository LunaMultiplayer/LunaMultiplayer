using LunaClient.Windows.Chat;
using System;
using LunaClient.Windows;

namespace LunaClient.Systems.Chat.Command
{
    public class ResizeCommand
    {
        public void ResizeChat(string commandArgs)
        {
            float size = 0;

            var func = commandArgs;
            if (commandArgs.Contains(" "))
            {
                func = commandArgs.Substring(0, commandArgs.IndexOf(" ", StringComparison.Ordinal));
                if (commandArgs.Substring(func.Length).Contains(" "))
                {
                    try
                    {
                        size = Convert.ToSingle(commandArgs.Substring(func.Length + 1));
                    }
                    catch (FormatException)
                    {
                        SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel($"Error: {size} is not a valid number");
                        size = 400f;
                    }
                }
            }

            switch (func)
            {
                default:
                    SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel("Undefined function. Usage: /resize [default|medium|large], /resize [x|y] size, or /resize show");
                    SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel($"Chat window size is currently: {WindowsContainer.Get<ChatWindow>().WindowWidth}x{WindowsContainer.Get<ChatWindow>().WindowHeight}");
                    break;
                case "x":
                    SetWindowSize(size);
                    break;
                case "y":
                    SetWindowSize(null, size);
                    break;
                case "default":
                    SetWindowSize(400, 300);
                    break;
                case "medium":
                    SetWindowSize(600, 600);
                    break;
                case "large":
                    SetWindowSize(800, 800);
                    break;
                case "show":
                    SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel($"Chat window size is currently: {WindowsContainer.Get<ChatWindow>().WindowWidth}x{WindowsContainer.Get<ChatWindow>().WindowHeight}");
                    break;
            }
        }

        private void SetWindowSize(float? x = null, float? y = null)
        {
            var sizeChanged = false;
            if (x.HasValue && x <= 800 && x >= 300)
            {
                WindowsContainer.Get<ChatWindow>().WindowWidth = x.Value;
                sizeChanged = true;
            }

            if (y.HasValue && y <= 800 && y >= 300)
            {
                WindowsContainer.Get<ChatWindow>().WindowHeight = y.Value;
                sizeChanged = true;
            }

            if (y.HasValue && (y > 800 || y < 300) || x.HasValue && (x > 800 || x < 300))
                SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel("Size is out of range.");

            if (sizeChanged)
            {
                WindowsContainer.Get<ChatWindow>().SizeChanged();
            }
        }
    }
}