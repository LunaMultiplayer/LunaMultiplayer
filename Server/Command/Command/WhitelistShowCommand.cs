using LunaServer.Command.Command.Base;
using LunaServer.Log;

namespace LunaServer.Command.Command
{
    public class WhitelistShowCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            foreach (var player in Retrieve())
                LunaLog.Normal(player);
        }
    }
}