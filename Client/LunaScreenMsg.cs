using System;
using System.Collections.Concurrent;

namespace LunaClient
{
    /// <summary>
    /// Use this class to post screen messages in a thread safety way. You can call PostScreenMessage from any thread without worrying about unity
    /// </summary>
    public class LunaScreenMsg
    {
        #region Helper classes

        private class ScreenMsgEntry
        {
            public string Text { get; }
            public float Duration { get; }
            public ScreenMessageStyle Location { get; }

            public ScreenMsgEntry(string text, float durationInSeconds, ScreenMessageStyle location)
            {
                Text = text;
                Duration = durationInSeconds;
                Location = location;
            }
        }

        #endregion

        #region Fields & properties

        private static readonly ConcurrentQueue<ScreenMsgEntry> Queue = new ConcurrentQueue<ScreenMsgEntry>();

        #endregion

        #region Posting msg

        /// <summary>
        /// Posts a message in the screen
        /// </summary>
        public static ScreenMessage PostScreenMessage(string text, float durationInSeconds, ScreenMessageStyle location)
        {
            if (MainSystem.IsUnityThread)
            {
                return ScreenMessages.PostScreenMessage(text, durationInSeconds, location);
            }

            Queue.Enqueue(new ScreenMsgEntry(text, durationInSeconds, location));

            return null;
        }

        #endregion

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
                ScreenMessages.PostScreenMessage(entry.Text, entry.Duration, entry.Location);
            }
        }

        #endregion
    }
}
