using LMP.Server.Command.Command.Base;
using LMP.Server.Context;
using LMP.Server.Log;

namespace LMP.Server.Command.Command
{
    public class CountClientsCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            LunaLog.Normal($"Online Players: {ServerContext.PlayerCount}");
        }
    }
}