using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;

namespace LunaServer.Command.Command
{
    public class CountClientsCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            LunaLog.Normal("Online Players: " + ServerContext.PlayerCount);
        }
    }
}