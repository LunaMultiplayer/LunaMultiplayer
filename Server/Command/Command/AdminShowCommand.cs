using Server.Command.Command.Base;
using Server.Log;

namespace Server.Command.Command
{
    public class AdminShowCommand : AdminCommand
    {
        public override void Execute(string commandArgs)
        {
            foreach (var player in Retrieve())
                LunaLog.Normal(player);
        }
    }
}