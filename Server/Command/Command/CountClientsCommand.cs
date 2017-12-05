using Server.Command.Command.Base;
using Server.Context;
using Server.Log;

namespace Server.Command.Command
{
    public class CountClientsCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            LunaLog.Normal($"Online Players: {ServerContext.PlayerCount}");
        }
    }
}