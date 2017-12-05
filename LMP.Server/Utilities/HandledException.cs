using System;

namespace LMP.Server.Utilities
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
