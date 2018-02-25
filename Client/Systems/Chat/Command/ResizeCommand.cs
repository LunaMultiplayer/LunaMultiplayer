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
                        ChatSystem.Singleton.PrintToSelectedChannel($"Error: {size} is not a valid number");
                        size = 400f;
                    }
                }
            }

            switch (func)
            {
                default:
                    ChatSystem.Singleton.PrintToSelectedChannel("Undefined function. Usage: /resize [default|medium|large], /resize [x|y] size, or /resize show");
                    ChatSystem.Singleton.PrintToSelectedChannel($"Chat window size is currently: {ChatWindow.Singleton.WindowWidth}x{ChatWindow.Singleton.WindowHeight}");
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
                    ChatSystem.Singleton.PrintToSelectedChannel($"Chat window size is currently: {ChatWindow.Singleton.WindowWidth}x{ChatWindow.Singleton.WindowHeight}");
                    break;
            }
        }

        private void SetWindowSize(float? x = null, float? y = null)
        {
            var sizeChanged = false;
            if (x.HasValue && x <= 800 && x >= 300)
            {
                ChatWindow.Singleton.WindowWidth = x.Value;
                sizeChanged = true;
            }

            if (y.HasValue && y <= 800 && y >= 300)
            {
                ChatWindow.Singleton.WindowHeight = y.Value;
                sizeChanged = true;
            }

            if (y.HasValue && (y > 800 || y < 300) || x.HasValue && (x > 800 || x < 300))
                ChatSystem.Singleton.PrintToSelectedChannel("Size is out of range.");

            if (sizeChanged)
            {
                ChatWindow.Singleton.SizeChanged();
            }
        }
    }
}