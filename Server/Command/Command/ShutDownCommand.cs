using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;

namespace Server.Command.Command
{
    public class ShutDownCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            if (commandArgs != "")
            {
                LunaLog.Normal($"Shutting down - {commandArgs}");
                MessageQueuer.SendConnectionEndToAll($"Server is shutting down - {commandArgs}");
            }
            else
            {
                LunaLog.Normal("Shutting down");
                MessageQueuer.SendConnectionEndToAll("Server is shutting down");
            }
            ServerContext.ServerStarting = false;
            ServerContext.ServerRunning = false;
        }
    }
}