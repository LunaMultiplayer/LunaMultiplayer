using LMP.Server.Command.Command.Base;
using LMP.Server.Context;
using LMP.Server.Log;

namespace LMP.Server.Command.Command
{
    public class ListClientsCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            if (ServerContext.Players != "")
                LunaLog.Normal($"Online Players: {ServerContext.Players}");
            else
                LunaLog.Normal("No Clients connected");
        }
    }
}