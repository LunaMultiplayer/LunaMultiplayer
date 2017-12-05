using LMP.Server.Command.Command.Base;
using LMP.Server.Log;

namespace LMP.Server.Command.Command
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