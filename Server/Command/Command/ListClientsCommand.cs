using Server.Command.Command.Base;
using Server.Context;
using Server.Log;

namespace Server.Command.Command
{
    public class ListClientsCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            LunaLog.Normal(ServerContext.Players != "" ? $"Online Players: {ServerContext.Players}" : "No Clients connected");
            return true;
        }
    }
}
