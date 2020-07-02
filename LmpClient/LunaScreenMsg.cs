using System;
using System.Collections.Concurrent;
using KSP.UI.Screens;
using UnityEngine;

namespace LmpClient
{
    /// <summary>
    /// Use this class to post screen messages in a thread safety way. You can call PostScreenMessage from any thread without worrying about unity
    /// </summary>
    public class LunaScreenMsg
    {

        #region Fields & properties

        private static readonly ConcurrentQueue<ScreenMessage> Queue = new ConcurrentQueue<ScreenMessage>();

        #endregion

        #region Posting msg

        /// <summary>
        /// Posts a message in the screen
        /// </summary>
        public static ScreenMessage PostScreenMessage(string text, float durationInSeconds, ScreenMessageStyle location)
        {
            return PostScreenMessage(text, durationInSeconds, location, Color.green);
        }

        /// <summary>
        /// Posts a message in the screen
        /// </summary>
        public static ScreenMessage PostScreenMessage(string text, float durationInSeconds, ScreenMessageStyle location, Color color)
        {
            if (MainSystem.IsUnityThread)
            {
                return ScreenMessages.PostScreenMessage(text, durationInSeconds, location, color);
            }

            Queue.Enqueue(CreateMessage(text, durationInSeconds, location, color));

            return null;
        }

        #endregion

        private static ScreenMessage CreateMessage(string text, float durationInSeconds, ScreenMessageStyle location, Color color)
        {
            return new ScreenMessage(text, durationInSeconds, location)
            {
                color = color
            };
        }

        #region Process

        /// <summary>
        /// Call this method FROM the unity thread so it reads all the queued screen messages and prints them
        /// </summary>
        public static void ProcessScreenMessages()
        {
            if (!MainSystem.IsUnityThread)
            {
                throw new Exception("Cannot call ProcessScreenMessages from another thread that is not the Unity thread");
            }

            while (Queue.TryDequeue(out var entry))
            {
                ScreenMessages.PostScreenMessage(entry);
            }
        }

        #endregion
    }
}
