using LmpCommon;

namespace LmpMasterServer.Log
{
    public class LunaLog : BaseLogger
    {
        private static readonly BaseLogger Singleton = new LunaLog();

        #region Public methods

        public new static void NetworkVerboseDebug(string message)
        {
            Singleton.NetworkVerboseDebug(message);
        }

        public new static void NetworkDebug(string message)
        {
            Singleton.NetworkDebug(message);
        }

        public new static void Debug(string message)
        {
            Singleton.Debug(message);
        }

        public new static void Warning(string message)
        {
            Singleton.Warning(message);
        }

        public new static void Info(string message)
        {
            Singleton.Info(message);
        }

        public new static void Normal(string message)
        {
            Singleton.Normal(message);
        }

        public new static void Error(string message)
        {
            Singleton.Error(message);
        }

        public new static void Fatal(string message)
        {
            Singleton.Fatal(message);
        }

        public new static void ChatMessage(string message)
        {
            Singleton.ChatMessage(message);
        }

        #endregion
    }
}
