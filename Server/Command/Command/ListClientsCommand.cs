using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;

namespace LunaServer.Command.Command
{
    public class ListClientsCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            if (ServerContext.Players != "")
                LunaLog.Normal("Online Players: " + ServerContext.Players);
            else
                LunaLog.Normal("No Clients connected");
        }
    }
}