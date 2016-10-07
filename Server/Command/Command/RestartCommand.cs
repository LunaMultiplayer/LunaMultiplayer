using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.Command.Command
{
    public class RestartCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            if (commandArgs != "")
            {
                LunaLog.Normal("Restarting - " + commandArgs);
                MessageQueuer.SendConnectionEndToAll("Server is restarting - " + commandArgs);
            }
            else
            {
                LunaLog.Normal("Restarting");
                MessageQueuer.SendConnectionEndToAll("Server is restarting");
            }
            ServerContext.ServerRestarting = true;
            ServerContext.ServerStarting = false;
            ServerContext.ServerRunning = false;
        }
    }
}