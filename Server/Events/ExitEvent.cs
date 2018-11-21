namespace Server.Events
{
    /// <summary>
    /// Subscribe to this event if you want to do some functionality before closing
    /// </summary>
    public static class ExitEvent
    {
        public delegate void ExitHandler();

        public static event ExitHandler ServerClosing;

        public static void Exit()
        {
            ServerClosing?.Invoke();
        }
    }
}
