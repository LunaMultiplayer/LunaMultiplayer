using LunaServer.Command.Command.Base;
using LunaServer.Log;

namespace LunaServer.Command.Command
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