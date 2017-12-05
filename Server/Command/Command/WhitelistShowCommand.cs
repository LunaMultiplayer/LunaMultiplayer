using Server.Command.Command.Base;
using Server.Log;

namespace Server.Command.Command
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