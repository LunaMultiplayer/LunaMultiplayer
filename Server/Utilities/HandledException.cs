using System;

namespace LunaServer.Utilities
{
    internal sealed class HandledException : Exception
    {
        public override string Message { get; }

        public HandledException(string message)
        {
            Message = message;
        }
    }
}
