using LMP.Server.Command.Command.Base;
using LMP.Server.Log;

namespace LMP.Server.Command.Command
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