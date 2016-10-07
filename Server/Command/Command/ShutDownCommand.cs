using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.Command.Command
{
    public class ShutDownCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            if (commandArgs != "")
            {
                LunaLog.Normal("Shutting down - " + commandArgs);
                MessageQueuer.SendConnectionEndToAll("Server is shutting down - " + commandArgs);
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