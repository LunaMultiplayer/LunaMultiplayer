using Server.Command.Command.Base;
using Server.Context;
using Server.Log;

namespace Server.Command.Command
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